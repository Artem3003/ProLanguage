# Azure Key Vault setup for ProLanguage.
#
# Creates a Key Vault and populates it with the secrets the .NET apps need at runtime.
# The apps read secrets directly via `builder.Configuration.AddAzureKeyVault(...)` in Program.cs,
# authenticating via DefaultAzureCredential (managed identity in Azure, az-login locally).
#
# Secret naming: Key Vault names use `--` (double-dash) where .NET config uses `:`.
#   Key Vault:  ConnectionStrings--AuthConnection
#   .NET reads: Configuration["ConnectionStrings:AuthConnection"]
#
# Prerequisites:
#   - Azure CLI installed and `az login` complete
#   - Caller has rights to create Key Vault + assign roles in the subscription
#
# Usage:
#   1. Fill in the secret values at the top (use the same values you'd put in user-secrets locally)
#   2. .\infra\azure-keyvault-setup.ps1
#   3. Rename this file to azure-keyvault-setup.local.ps1 (gitignored) before running with real values

param(
    [string]$ResourceGroup = "rg-prolanguage-prod-plc-001",
    [string]$Location = "polandcentral",
    [string]$KeyVaultName = "kv-prolanguage"
)

$ErrorActionPreference = "Stop"

function Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

# ==== FILL THESE IN WITH ROTATED VALUES (or leave blank to skip setting) ====
$secrets = [ordered]@{
    "ConnectionStrings--AuthConnection"     = ""
    "ConnectionStrings--PaymentsConnection" = ""
    "ConnectionStrings--DefaultConnection"  = ""
    "ConnectionStrings--ServiceBus"         = ""
    "JwtSettings--SecretKey"                = ""
    "GoogleAuth--ClientSecret"              = ""
    "EmailSettings--SmtpPassword"           = ""
    "AzureBlobStorage--ConnectionString"    = ""
    "AzureOpenAI--ApiKey"                   = ""
}
# ============================================================================

Step "Verifying az login"
$subId = az account show --query id -o tsv
if (-not $subId) { throw "Not logged in. Run 'az login' first." }
$callerObjectId = az ad signed-in-user show --query id -o tsv
Write-Host "Subscription: $subId"
Write-Host "Caller object id: $callerObjectId"

Step "Verifying resource group ($ResourceGroup)"
$rgExists = az group show -n $ResourceGroup --query name -o tsv 2>$null
if (-not $rgExists) {
    Write-Host "Creating resource group" -ForegroundColor Yellow
    az group create -n $ResourceGroup -l $Location -o none
}

Step "Creating Key Vault ($KeyVaultName)"
$kvExists = az keyvault show -n $KeyVaultName --query name -o tsv 2>$null
if (-not $kvExists) {
    # --enable-rbac-authorization uses Azure RBAC instead of legacy access policies
    az keyvault create `
        -n $KeyVaultName `
        -g $ResourceGroup `
        -l $Location `
        --enable-rbac-authorization true `
        --enable-purge-protection true `
        --retention-days 7 `
        -o none
} else {
    Write-Host "Key Vault already exists, skipping creation"
}

$kvUri = az keyvault show -n $KeyVaultName --query properties.vaultUri -o tsv
Write-Host "Key Vault URI: $kvUri"

Step "Granting caller 'Key Vault Secrets Officer' role (needed to write secrets)"
$kvId = az keyvault show -n $KeyVaultName --query id -o tsv
az role assignment create `
    --assignee-object-id $callerObjectId `
    --assignee-principal-type User `
    --role "Key Vault Secrets Officer" `
    --scope $kvId -o none 2>$null

Step "Waiting 30s for role assignment to propagate"
Start-Sleep -Seconds 30

Step "Setting secrets"
foreach ($key in $secrets.Keys) {
    $value = $secrets[$key]
    if ([string]::IsNullOrWhiteSpace($value)) {
        Write-Host "  $key  [SKIP — no value provided]" -ForegroundColor DarkGray
        continue
    }
    Write-Host "  $key"
    az keyvault secret set --vault-name $KeyVaultName --name $key --value $value -o none
}

Step "Granting Container App managed identities access to Key Vault"
$apps = @('prolanguage-auth', 'prolanguage-payments', 'prolanguage-web')
foreach ($app in $apps) {
    $appExists = az containerapp show -n $app -g $ResourceGroup --query name -o tsv 2>$null
    if (-not $appExists) {
        Write-Host "  $app  [SKIP — Container App not found; run azure-bootstrap.ps1 first]" -ForegroundColor DarkGray
        continue
    }
    $principalId = az containerapp identity show -n $app -g $ResourceGroup --query principalId -o tsv
    if (-not $principalId) {
        Write-Host "  $app  [assigning system-assigned identity]"
        az containerapp identity assign -n $app -g $ResourceGroup --system-assigned -o none
        $principalId = az containerapp identity show -n $app -g $ResourceGroup --query principalId -o tsv
    }
    Write-Host "  $app  -> grant 'Key Vault Secrets User' + set KeyVault__Endpoint"
    az role assignment create `
        --assignee-object-id $principalId `
        --assignee-principal-type ServicePrincipal `
        --role "Key Vault Secrets User" `
        --scope $kvId -o none 2>$null
    az containerapp update -n $app -g $ResourceGroup `
        --set-env-vars "KeyVault__Endpoint=$kvUri" -o none
}

Step "Done"
Write-Host ""
Write-Host "Key Vault URI: $kvUri"
Write-Host "Container Apps now read secrets directly from Key Vault via managed identity."
