// Fato principal: um registro por e-mail classificado.
// Ajuste Server/Database antes de colar no Editor Avançado do Power Query.
let
    Source = PostgreSQL.Database("localhost", "phishing_detection"),
    public_emails = Source{[Schema="public",Item="emails"]}[Data],
    #"Changed Type" = Table.TransformColumnTypes(public_emails, {
        {"id", Int64.Type},
        {"message_id", type text},
        {"subject", type text},
        {"sender", type text},
        {"recipients", type text},
        {"received_date", type datetime},
        {"processed_date", type datetime},
        {"classification", type text},
        {"confidence", type number},
        {"is_validated", type logical},
        {"user_feedback", type text},
        {"created_at", type datetime},
        {"updated_at", type datetime}
    }),
    #"Removed Body Columns" = Table.RemoveColumns(#"Changed Type", {"email_body", "email_headers"}),
    #"Added ClassificationLabel" = Table.AddColumn(#"Removed Body Columns", "ClassificationLabel", each
        if [classification] = "safe" then "Seguro"
        else if [classification] = "suspicious" then "Suspeito"
        else if [classification] = "phishing" then "Phishing"
        else [classification], type text),
    #"Added ReceivedDateOnly" = Table.AddColumn(#"Added ClassificationLabel", "ReceivedDateOnly", each Date.From([received_date]), type date)
in
    #"Added ReceivedDateOnly"
