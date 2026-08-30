// Dimensão de usuários — usada em relatórios de auditoria e consentimento LGPD.
// Nunca traga password_hash para o modelo do Power BI.
let
    Source = PostgreSQL.Database("localhost", "phishing_detection"),
    public_users = Source{[Schema="public",Item="users"]}[Data],
    #"Removed Sensitive Columns" = Table.RemoveColumns(public_users, {"password_hash"}),
    #"Changed Type" = Table.TransformColumnTypes(#"Removed Sensitive Columns", {
        {"id", Int64.Type},
        {"username", type text},
        {"email", type text},
        {"full_name", type text},
        {"department", type text},
        {"role", type text},
        {"is_active", type logical},
        {"last_login", type datetime}
    })
in
    #"Changed Type"
