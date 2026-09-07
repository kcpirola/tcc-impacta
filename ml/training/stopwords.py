"""
Listas de stopwords em português e inglês, embarcadas no repositório.

Optou-se por uma lista estática (em vez de depender do NLTK em runtime) para que o
pré-processamento funcione offline, em CI e sem passo extra de download. As listas são as
mesmas distribuídas pelo NLTK (`nltk.corpus.stopwords`) para os dois idiomas, já que os
e-mails do dataset misturam português e inglês.
"""

PT_STOPWORDS: frozenset[str] = frozenset(
    {
        "a", "à", "ao", "aos", "aquela", "aquelas", "aquele", "aqueles", "aquilo", "as",
        "às", "até", "com", "como", "da", "das", "de", "dela", "delas", "dele", "deles",
        "depois", "do", "dos", "e", "ela", "elas", "ele", "eles", "em", "entre", "era",
        "eram", "éramos", "essa", "essas", "esse", "esses", "esta", "está", "estamos",
        "estão", "estas", "estava", "estavam", "estávamos", "este", "esteja", "estejam",
        "estejamos", "estes", "esteve", "estive", "estivemos", "estiver", "estivera",
        "estiveram", "estivéramos", "estiverem", "estivermos", "estivesse", "estivessem",
        "estivéssemos", "estou", "eu", "foi", "fomos", "for", "fora", "foram", "fôramos",
        "forem", "formos", "fosse", "fossem", "fôssemos", "fui", "há", "haja", "hajam",
        "hajamos", "hão", "havemos", "haver", "hei", "houve", "houvemos", "houver",
        "houvera", "houverá", "houveram", "houvéramos", "houverão", "houverei", "houverem",
        "houveremos", "houveria", "houveriam", "houveríamos", "houvermos", "houvesse",
        "houvessem", "houvéssemos", "isso", "isto", "já", "lhe", "lhes", "mais", "mas",
        "me", "mesmo", "meu", "meus", "minha", "minhas", "muito", "na", "não", "nas", "nem",
        "no", "nos", "nós", "nossa", "nossas", "nosso", "nossos", "num", "numa", "o", "os",
        "ou", "para", "pela", "pelas", "pelo", "pelos", "por", "qual", "quando", "que",
        "quem", "são", "se", "seja", "sejam", "sejamos", "sem", "ser", "será", "serão",
        "serei", "seremos", "seria", "seriam", "seríamos", "seu", "seus", "só", "somos",
        "sou", "sua", "suas", "também", "te", "tem", "tém", "temos", "tenha", "tenham",
        "tenhamos", "tenho", "terá", "terão", "terei", "teremos", "teria", "teriam",
        "teríamos", "teu", "teus", "teve", "tinha", "tinham", "tínhamos", "tive", "tivemos",
        "tiver", "tivera", "tiveram", "tivéramos", "tiverem", "tivermos", "tivesse",
        "tivessem", "tivéssemos", "tu", "tua", "tuas", "um", "uma", "você", "vocês", "vos",
    }
)

EN_STOPWORDS: frozenset[str] = frozenset(
    {
        "a", "about", "above", "after", "again", "against", "ain", "all", "am", "an", "and",
        "any", "are", "aren", "as", "at", "be", "because", "been", "before", "being",
        "below", "between", "both", "but", "by", "can", "couldn", "d", "did", "didn", "do",
        "does", "doesn", "doing", "don", "down", "during", "each", "few", "for", "from",
        "further", "had", "hadn", "has", "hasn", "have", "haven", "having", "he", "her",
        "here", "hers", "herself", "him", "himself", "his", "how", "i", "if", "in", "into",
        "is", "isn", "it", "its", "itself", "just", "ll", "m", "ma", "me", "mightn", "more",
        "most", "mustn", "my", "myself", "needn", "no", "nor", "not", "now", "o", "of",
        "off", "on", "once", "only", "or", "other", "our", "ours", "ourselves", "out",
        "over", "own", "re", "s", "same", "shan", "she", "should", "shouldn", "so", "some",
        "such", "t", "than", "that", "the", "their", "theirs", "them", "themselves", "then",
        "there", "these", "they", "this", "those", "through", "to", "too", "under", "until",
        "up", "ve", "very", "was", "wasn", "we", "were", "weren", "what", "when", "where",
        "which", "while", "who", "whom", "why", "will", "with", "won", "wouldn", "y", "you",
        "your", "yours", "yourself", "yourselves",
    }
)

# União usada pelo pré-processamento (os e-mails misturam os dois idiomas).
ALL_STOPWORDS: frozenset[str] = PT_STOPWORDS | EN_STOPWORDS
