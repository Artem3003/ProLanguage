using Prometheus;

namespace Application.Metrics;

public static class BusinessMetrics
{
    public static readonly Counter CoursesViewedTotal = Prometheus.Metrics.CreateCounter(
        "courses_viewed_total",
        "Total number of course detail page views");

    public static readonly Counter OrdersCreatedTotal = Prometheus.Metrics.CreateCounter(
        "orders_created_total",
        "Total number of orders created",
        new CounterConfiguration { LabelNames = ["status"] });

    public static readonly Counter PaymentsProcessedTotal = Prometheus.Metrics.CreateCounter(
        "payments_processed_total",
        "Total number of payment attempts",
        new CounterConfiguration { LabelNames = ["method", "result"] });

    public static readonly Gauge ActiveChatSessions = Prometheus.Metrics.CreateGauge(
        "active_chat_sessions",
        "Number of currently active AI chat sessions");

    public static readonly Counter CommentsPostedTotal = Prometheus.Metrics.CreateCounter(
        "comments_posted_total",
        "Total number of comments posted");

    public static readonly Counter UsersBannedTotal = Prometheus.Metrics.CreateCounter(
        "users_banned_total",
        "Total number of user bans applied",
        new CounterConfiguration { LabelNames = ["duration"] });
}
