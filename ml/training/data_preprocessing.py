"""
Módulo de pré-processamento de texto de e-mails.
Responsável por limpar, normalizar e preparar o texto para extração de features.
"""
import re
import html
import unicodedata

from bs4 import BeautifulSoup

from training.stopwords import ALL_STOPWORDS

URL_PATTERN = re.compile(r"https?://\S+|www\.\S+", re.IGNORECASE)
EMAIL_PATTERN = re.compile(r"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}")
MULTIPLE_SPACES_PATTERN = re.compile(r"\s+")
NON_ALPHANUMERIC_PATTERN = re.compile(r"[^a-zA-Z0-9À-ÿ\s]")


class EmailPreprocessor:
    """
    Responsável exclusivamente por limpar e normalizar o texto de e-mails.
    Segue o Princípio da Responsabilidade Única (SRP): não extrai features,
    apenas prepara o texto bruto para as etapas seguintes do pipeline.
    """

    @staticmethod
    def strip_html(text: str) -> str:
        """Remove tags HTML e retorna apenas o texto visível."""
        if not text:
            return ""
        soup = BeautifulSoup(text, "html.parser")
        return soup.get_text(separator=" ")

    @staticmethod
    def extract_urls(text: str) -> list[str]:
        """Extrai todas as URLs presentes no texto."""
        if not text:
            return []
        return URL_PATTERN.findall(text)

    @staticmethod
    def remove_urls(text: str) -> str:
        """Remove URLs do texto, substituindo por um marcador."""
        return URL_PATTERN.sub(" URLTOKEN ", text)

    @staticmethod
    def remove_emails(text: str) -> str:
        """Remove endereços de e-mail do texto, substituindo por um marcador."""
        return EMAIL_PATTERN.sub(" EMAILTOKEN ", text)

    @staticmethod
    def normalize_unicode(text: str) -> str:
        """Normaliza caracteres unicode (remove acentos duplicados, etc)."""
        text = html.unescape(text)
        return unicodedata.normalize("NFKC", text)

    @staticmethod
    def to_lowercase(text: str) -> str:
        """Converte o texto para minúsculas."""
        return text.lower()

    @staticmethod
    def remove_extra_whitespace(text: str) -> str:
        """Remove espaços em branco duplicados."""
        return MULTIPLE_SPACES_PATTERN.sub(" ", text).strip()

    @staticmethod
    def remove_stopwords(text: str) -> str:
        """
        Remove stopwords em português e inglês de um texto já tokenizável por espaço.

        Espera o texto em minúsculas e sem pontuação (etapa final do `clean`). Palavras
        de conteúdo são preservadas; conectivos e artigos comuns aos dois idiomas são
        descartados, seguindo a metodologia ("eliminação de stopwords").
        """
        if not text:
            return ""
        return " ".join(word for word in text.split() if word not in ALL_STOPWORDS)

    def clean(self, subject: str, body: str, remove_stops: bool = True) -> str:
        """
        Executa o pipeline completo de limpeza para classificação textual.

        Args:
            subject: Assunto do e-mail.
            body: Corpo do e-mail (pode conter HTML).
            remove_stops: Se True (padrão), remove stopwords PT+EN ao final do pipeline.

        Returns:
            Texto limpo e normalizado, pronto para vetorização TF-IDF.
        """
        combined = f"{subject or ''} {body or ''}"
        combined = self.strip_html(combined)
        combined = self.normalize_unicode(combined)
        combined = self.remove_urls(combined)
        combined = self.remove_emails(combined)
        combined = self.to_lowercase(combined)
        combined = NON_ALPHANUMERIC_PATTERN.sub(" ", combined)
        combined = self.remove_extra_whitespace(combined)
        if remove_stops:
            combined = self.remove_stopwords(combined)
        return combined
