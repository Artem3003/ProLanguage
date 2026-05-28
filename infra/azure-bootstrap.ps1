# Azure Container Apps bootstrap for ProLanguage.
# Idempotent: safe to re-run. Creates: resource group, ACR, Container Apps environment,
# 4 container apps (Auth, Payments, Web, UI), and a service principal for GitHub Actions.
#
# Prerequisites:
#   - Azure CLI installed (`winget install -e --id Microsoft.AzureCLI`)
#   - Logged in (`az login`)
#   - Correct subscription selected (`az account set --subscription <id>`)
#
# Usage:
#   .\infra\azure-bootstrap.ps1
#   .\infra\azure-bootstrap.ps1 -ResourceGroup my-rg -Location westeurope -AcrName myacr

param(
    [string]$ResourceGroup = "rg-prolanguage-prod-plc-001",
    [string]$Location = "polandcentral",
    [string]$AcrName = "prolanguage",
    [string]$EnvName = "prolanguage-env",
    [string]$SpName = "prolanguage-gha-sp"
)

$ErrorActionPreference = "Stop"

function Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

Step "Verifying az login"
$subId = az account show --query id -o tsv
if (-not $subId) { throw "Not logged in. Run 'az login' first." }
Write-Host "Subscription: $subId"

Step "Installing containerapp extension if missing"
az extension add --name containerapp --upgrade --only-show-errors | Out-Null
az provider register --namespace Microsoft.App --wait | Out-Null
az provider register --namespace Microsoft.OperationalInsights --wait | Out-Null

Step "Creating resource group ($ResourceGroup in $Location)"
az group create -n $ResourceGroup -l $Location -o none

Step "Creating Azure Container Registry ($AcrName)"
$acrExists = az acr show -n $AcrName --query name -o tsv 2>$null
if (-not $acrExists) {
    az acr create -n $AcrName -g $ResourceGroup --sku Basic --admin-enabled false -o none
}
$acrLoginServer = az acr show -n $AcrName --query loginServer -o tsv
Write-Host "ACR login server: $acrLoginServer"

Step "Creating Container Apps environment ($EnvName)"
$envExists = az containerapp env show -n $EnvName -g $ResourceGroup --query name -o tsv 2>$null
if (-not $envExists) {
    az containerapp env create -n $EnvName -g $ResourceGroup -l $Location -o none
}

Step "Creating 4 container apps with placeholder image"
$apps = @(
    @{ Name = "prolanguage-auth";     Port = 8080; Ingress = "external" }
    @{ Name = "prolanguage-payments"; Port = 8080; Ingress = "external" }
    @{ Name = "prolanguage-web";      Port = 8080; Ingress = "external" }
    @{ Name = "prolanguage-ui";       Port = 80;   Ingress = "external" }
)
foreach ($app in $apps) {
    $exists = az containerapp show -n $app.Name -g $ResourceGroup --query name -o tsv 2>$null
    if (-not $exists) {
        Write-Host "  Creating $($app.Name)"
        az containerapp create `
            -n $app.Name `
            -g $ResourceGroup `
            --environment $EnvName `
            --image "mcr.microsoft.com/k8se/quickstart:latest" `
            --target-port $app.Port `
            --ingress $app.Ingress `
            --min-replicas 0 `
            --max-replicas 3 `
            -o none
    } else {
        Write-Host "  $($app.Name) already exists, skipping"
    }
}

Step "Creating service principal for GitHub Actions ($SpName)"
$scope = "/subscriptions/$subId/resourceGroups/$ResourceGroup"
$spJson = az ad sp create-for-rbac `
    --name $SpName `
    --role contributor `
    --scopes $scope `
    --json-auth
$sp = $spJson | ConvertFrom-Json
$spAppId = $sp.clientId

Step "Granting AcrPush on the registry"
$acrId = az acr show -n $AcrName --query id -o tsv
az role assignment create --assignee $spAppId --role AcrPush --scope $acrId -o none 2>$null

Step "Granting managed identity for ACR pull on each container app"
foreach ($app in $apps) {
    az containerapp identity assign -n $app.Name -g $ResourceGroup --system-assigned -o none
    $principalId = az containerapp identity show -n $app.Name -g $ResourceGroup --query principalId -o tsv
    az role assignment create --assignee $principalId --role AcrPull --scope $acrId -o none 2>$null
    az containerapp registry set -n $app.Name -g $ResourceGroup --server $acrLoginServer --identity system -o none
}

Step "Done"
Write-Host ""
Write-Host "=== Add these to GitHub repo Secrets (Settings -> Secrets and variables -> Actions) ===" -ForegroundColor Yellow
Write-Host ""
Write-Host "AZURE_CREDENTIALS:" -ForegroundColor Yellow
Write-Host $spJson
Write-Host ""
Write-Host "ACR_NAME: $AcrName"
Write-Host "AZURE_RESOURCE_GROUP: $ResourceGroup"
Write-Host ""
Write-Host "=== Next: create Key Vault and grant access ===" -ForegroundColor Yellow
Write-Host "Run: .\infra\azure-keyvault-setup.ps1"
Write-Host "(That script creates the KV, populates secrets, grants managed-identity access, and sets KeyVault__Endpoint env var on each app.)"
