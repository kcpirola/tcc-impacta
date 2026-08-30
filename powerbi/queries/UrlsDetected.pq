// URLs extraídas dos e-mails e seu score de reputação.
let
    Source = PostgreSQL.Database("localhost", "phishing_detection"),
    public_urls_detected = Source{[Schema="public",Item="urls_detected"]}[Data],
    #"Changed Type" = Table.TransformColumnTypes(public_urls_detected, {
        {"id", Int64.Type},
        {"email_id", Int64.Type},
        {"url", type text},
        {"is_suspicious", type logical},
        {"reputation_score", type number},
        {"checked_at", type datetime}
    })
in
    #"Changed Type"
