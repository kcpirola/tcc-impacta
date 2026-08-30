// Indicadores de phishing detectados por e-mail (explica a classificação).
let
    Source = PostgreSQL.Database("localhost", "phishing_detection"),
    public_phishing_indicators = Source{[Schema="public",Item="phishing_indicators"]}[Data],
    #"Changed Type" = Table.TransformColumnTypes(public_phishing_indicators, {
        {"id", Int64.Type},
        {"email_id", Int64.Type},
        {"indicator_type", type text},
        {"indicator_value", type text},
        {"risk_score", type number},
        {"created_at", type datetime}
    })
in
    #"Changed Type"
