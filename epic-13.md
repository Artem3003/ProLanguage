# Epic 13 - Metrics with Prometheus

Implement metrics collection and monitoring for the ProLanguage platform using Prometheus.

## General Requirements
Please use the following Angular Front-end: [prolanguage-ui-app](prolanguage-ui-app)

System should support the following features:
* Expose application metrics via a dedicated endpoint.
* Collect HTTP request metrics (count, duration, status codes).
* Collect business-level metrics (course views, active users, orders placed).
* Visualize metrics using a Prometheus-compatible dashboard.

### Technical Specifications
* Use the `prometheus-net` NuGet package to instrument the ASP.NET Core application.
* Metrics must be exposed at the standard `/metrics` endpoint.
* Use Prometheus scrape configuration to collect metrics at a regular interval.
* Optionally connect Prometheus to Grafana for dashboard visualization.

---

## Task Description

### E13 US1 - User Story 1

Expose a Prometheus metrics endpoint.

```xml
Url: /metrics
Type: GET
Response: Prometheus text-based exposition format (plain text)
```

The endpoint must be accessible by the Prometheus scraper and must not require authentication.

---

### E13 US2 - User Story 2

Collect and expose HTTP request metrics automatically for all API endpoints.

Metrics to include:

| Metric Name | Type | Description |
|---|---|---|
| `http_requests_total` | Counter | Total number of HTTP requests, labeled by method, endpoint, and status code |
| `http_request_duration_seconds` | Histogram | Duration of HTTP requests in seconds, labeled by method and endpoint |
| `http_requests_in_progress` | Gauge | Number of HTTP requests currently being processed |

Example output:
```
# HELP http_requests_total Total number of HTTP requests
# TYPE http_requests_total counter
http_requests_total{method="GET",endpoint="/courses",status_code="200"} 1042
http_requests_total{method="POST",endpoint="/orders/payment",status_code="400"} 7

# HELP http_request_duration_seconds HTTP request duration in seconds
# TYPE http_request_duration_seconds histogram
http_request_duration_seconds_bucket{method="GET",endpoint="/courses",le="0.1"} 980
http_request_duration_seconds_bucket{method="GET",endpoint="/courses",le="0.5"} 1030
http_request_duration_seconds_bucket{method="GET",endpoint="/courses",le="+Inf"} 1042
http_request_duration_seconds_sum{method="GET",endpoint="/courses"} 95.3
http_request_duration_seconds_count{method="GET",endpoint="/courses"} 1042
```

---

### E13 US3 - User Story 3

Collect and expose business-level metrics.

Metrics to include:

| Metric Name | Type | Description |
|---|---|---|
| `courses_viewed_total` | Counter | Total number of course detail page views |
| `orders_created_total` | Counter | Total number of orders created, labeled by status |
| `payments_processed_total` | Counter | Total number of payment attempts, labeled by method and result (`success`/`failure`) |
| `active_chat_sessions` | Gauge | Number of currently active AI chat sessions |
| `comments_posted_total` | Counter | Total number of comments posted |
| `users_banned_total` | Counter | Total number of user bans applied, labeled by duration |

Business metrics must be incremented at the appropriate points in the service layer (e.g., increment `courses_viewed_total` inside the course detail retrieval service method).

---

### E13 US4 - User Story 4

Collect and expose application health and infrastructure metrics.

Metrics to include:

| Metric Name | Type | Description |
|---|---|---|
| `dotnet_gc_collections_total` | Counter | Number of .NET garbage collection runs per generation |
| `dotnet_total_memory_bytes` | Gauge | Total memory allocated by the .NET runtime |
| `process_cpu_seconds_total` | Counter | Total CPU time consumed by the process |
| `db_query_duration_seconds` | Histogram | Duration of database queries, labeled by operation type (`read`/`write`) |

Use the built-in `prometheus-net` .NET runtime metrics collector where available; instrument database calls manually using a custom Histogram.

---

### E13 US5 - User Story 5

Configure a local Prometheus instance to scrape the application metrics endpoint.

Provide a `prometheus.yml` scrape configuration file:

```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'prolanguage-api'
    static_configs:
      - targets: ['localhost:5000']
```

The configuration file must be included in the repository alongside the application code.

---

## Non-functional Requirements

**E13 NFR1**
All metric names must follow the Prometheus naming convention: lowercase, words separated by underscores, suffixed with the unit where applicable (e.g., `_seconds`, `_bytes`, `_total`).

**E13 NFR2**
The `/metrics` endpoint must respond within 200 ms under normal load and must not block or slow down other API endpoints.

**E13 NFR3**
Use middleware-based instrumentation for HTTP metrics so that all endpoints are covered automatically without per-controller code changes.

**E13 NFR4** *(Optional)*
Connect Prometheus to a Grafana instance and create a dashboard with the following panels:
* Request rate (requests/second) per endpoint
* 95th percentile request duration per endpoint
* Total payment success vs failure rate
* Course views over time
* .NET memory usage and GC activity

**E13 NFR5** *(Optional)*
Configure Prometheus alerting rules for the following conditions:
* Error rate exceeds 5% of total requests over a 5-minute window
* Average request duration exceeds 2 seconds over a 5-minute window
* Application process memory exceeds 512 MB
