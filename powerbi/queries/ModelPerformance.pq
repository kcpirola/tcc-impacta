// Métricas de desempenho dos modelos ML (Naive Bayes, Random Forest, Ensemble) ao longo do tempo.
let
    Source = PostgreSQL.Database("localhost", "phishing_detection"),
    public_model_performance = Source{[Schema="public",Item="model_performance"]}[Data],
    #"Changed Type" = Table.TransformColumnTypes(public_model_performance, {
        {"id", Int64.Type},
        {"model_name", type text},
        {"model_version", type text},
        {"accuracy", type number},
        {"precision", type number},
        {"recall", type number},
        {"f1_score", type number},
        {"tested_emails", Int64.Type},
        {"tested_at", type datetime}
    }),
    #"Added TestedDateOnly" = Table.AddColumn(#"Changed Type", "TestedDateOnly", each Date.From([tested_at]), type date)
in
    #"Added TestedDateOnly"
