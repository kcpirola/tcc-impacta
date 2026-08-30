// Estatísticas agregadas por dia — base das séries temporais e KPIs do dashboard.
let
    Source = PostgreSQL.Database("localhost", "phishing_detection"),
    public_daily_statistics = Source{[Schema="public",Item="daily_statistics"]}[Data],
    #"Changed Type" = Table.TransformColumnTypes(public_daily_statistics, {
        {"id", Int64.Type},
        {"statistic_date", type date},
        {"total_emails_processed", Int64.Type},
        {"emails_classified_safe", Int64.Type},
        {"emails_classified_suspicious", Int64.Type},
        {"emails_classified_phishing", Int64.Type},
        {"avg_processing_time_ms", type number},
        {"model_accuracy", type number}
    })
in
    #"Changed Type"
