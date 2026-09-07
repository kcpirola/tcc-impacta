"""Testes unitários para EmailPreprocessor (limpeza e normalização de texto)."""
from training.data_preprocessing import EmailPreprocessor


class TestEmailPreprocessor:
    def setup_method(self):
        self.preprocessor = EmailPreprocessor()

    def test_strip_html_removes_tags_and_keeps_text(self):
        html = "<p>Olá <b>mundo</b></p>"
        result = self.preprocessor.strip_html(html)
        assert "<" not in result
        assert "Olá" in result and "mundo" in result

    def test_extract_urls_finds_all_urls(self):
        text = "Acesse http://exemplo.com ou www.outro.com agora"
        urls = self.preprocessor.extract_urls(text)
        assert len(urls) == 2

    def test_extract_urls_returns_empty_for_no_urls(self):
        assert self.preprocessor.extract_urls("Nenhuma URL aqui") == []

    def test_remove_urls_replaces_with_token(self):
        text = "Clique em http://malicious.com agora"
        result = self.preprocessor.remove_urls(text)
        assert "http://malicious.com" not in result
        assert "URLTOKEN" in result

    def test_remove_emails_replaces_with_token(self):
        text = "Contate joao@empresa.com para mais detalhes"
        result = self.preprocessor.remove_emails(text)
        assert "joao@empresa.com" not in result
        assert "EMAILTOKEN" in result

    def test_to_lowercase_converts_text(self):
        assert self.preprocessor.to_lowercase("URGENTE") == "urgente"

    def test_remove_extra_whitespace_collapses_spaces(self):
        assert self.preprocessor.remove_extra_whitespace("a    b   c") == "a b c"

    def test_clean_produces_normalized_lowercase_text_without_urls_or_html(self):
        subject = "URGENTE: Verifique sua conta"
        body = "<p>Clique aqui: http://192.168.1.1/login</p>"
        result = self.preprocessor.clean(subject, body)

        assert result == result.lower()
        assert "http" not in result
        assert "<" not in result
        assert "urgente" in result

    def test_clean_handles_empty_input_gracefully(self):
        assert self.preprocessor.clean("", "") == ""
        assert self.preprocessor.clean(None, None) == ""

    def test_remove_stopwords_strips_common_pt_and_en_words(self):
        text = "the account de o usuario foi bloqueado and you have to verify"
        result = self.preprocessor.remove_stopwords(text)
        tokens = result.split()
        # stopwords PT + EN removidas
        for stop in ("the", "de", "o", "foi", "and", "you", "have", "to"):
            assert stop not in tokens
        # palavras de conteúdo preservadas
        for keep in ("account", "usuario", "bloqueado", "verify"):
            assert keep in tokens

    def test_remove_stopwords_handles_empty_input(self):
        assert self.preprocessor.remove_stopwords("") == ""

    def test_clean_removes_stopwords_by_default(self):
        result = self.preprocessor.clean("Confirme a sua conta", "and you have to click here")
        tokens = result.split()
        for stop in ("a", "sua", "and", "you", "have", "to", "here"):
            assert stop not in tokens
        for keep in ("confirme", "conta", "click"):
            assert keep in tokens

    def test_clean_keeps_stopwords_when_disabled(self):
        result = self.preprocessor.clean("Confirme a sua conta", "", remove_stops=False)
        tokens = result.split()
        assert "a" in tokens
        assert "sua" in tokens
