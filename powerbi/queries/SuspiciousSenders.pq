// Tabela pré-agregada (view SQL) de remetentes suspeitos/phishing.
let
    Source = PostgreSQL.Database("localhost", "phishing_detection"),
    public_v_suspicious_senders = Source{[Schema="public",Item="v_suspicious_senders"]}[Data],
    #"Changed Type" = Table.TransformColumnTypes(public_v_suspicious_senders, {
        {"sender", type text},
        {"phishing_count", Int64.Type},
        {"confirmed_phishing", Int64.Type},
        {"last_occurrence", type datetime}
    })
in
    #"Changed Type"
