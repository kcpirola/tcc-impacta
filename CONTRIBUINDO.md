# Guia de Contribuição

## 📌 Importante

Este é um projeto acadêmico desenvolvido para conclusão de curso na Faculdade Impacta.

**Autores Originais:**
- Karyna da Costa Pirola
- Matheus Dias Yamasato
- Nicolas Kaue de Souza Rufino
- Suhelen Gonçalves Marques
- Thayssa Afonso Souza

**Orientador:** Prof. Me. Gilberto Alves Pereira

⚠️ **ATENÇÃO: Não adicionar novos contribuidores ao repositório sem autorização explícita do time original.**

---

## 🔒 Política de Contribuição

### Quem Pode Contribuir?

Este repositório é destinado a:
- ✅ Membros do time original (desenvolvedores listados acima)
- ✅ Supervisores e orientadores designados
- ✅ Equipe técnica da Health2B Hospitalar Ltda. (após aprovação)

**Contribuições externas não serão aceitas.**

### Como NÃO Contribuir

❌ **NÃO faça:**
- Fork do repositório para adicionar-se como autor
- Pull requests com alterações de copyright ou authorship
- Commits que modificam a lista de contribuidores
- Alterações no arquivo de autores/LICENSE sem permissão

### Fluxo de Desenvolvimento Interno

Se você é membro autorizado da equipe:

1. **Criar branch feature**
   ```bash
   git checkout -b feature/descricao-da-funcionalidade
   ```

2. **Seguir padrões de código**
   - Backend C#: Microsoft Coding Conventions
   - Python ML: PEP 8
   - Frontend React: Airbnb JavaScript Style Guide
   - Commits: Seguir Conventional Commits

3. **Exemplos de commit messages:**
   ```
   feat(backend): adicionar endpoint de classificação
   fix(ml): corrigir preprocessing de emails
   docs(readme): atualizar instruções de instalação
   test(api): adicionar testes unitários do controller
   refactor(frontend): reorganizar componentes
   ```

4. **Antes de fazer push:**
   - Executar testes: `npm test` ou `pytest`
   - Verificar linting: `eslint .` ou `pylint`
   - Atualizar documentação se necessário
   - Revisar mudanças: `git diff`

5. **Pull Request**
   - Descrever claramente o que foi implementado
   - Referenciar issue relacionada (se houver)
   - Aguardar review de pelo menos um membro
   - Manter as informações de autoria original

---

## 📋 Regras do Projeto (Baseadas no Artigo)

### Funcionalidades Obrigatórias

O sistema DEVE incluir:

- ✅ Classificação automática em 3 categorias (seguro, suspeito, phishing)
- ✅ Algoritmos Naive Bayes E Random Forest (ensemble)
- ✅ Integração com servidor IMAP/SMTP
- ✅ Alertas em tempo real via e-mail/dashboard
- ✅ Dashboard Power BI para relatórios
- ✅ Cálculo de métricas: acurácia, precisão, recall, F1-score
- ✅ Conformidade com LGPD
- ✅ Treinamento com datasets Sting9 + Kaggle
- ✅ Validação cruzada k-fold (k=10)

### Tecnologias Obrigatórias

- ✅ Backend: C# .NET 6+
- ✅ Frontend: React 18+
- ✅ ML: Python 3.9+ com scikit-learn
- ✅ Banco: PostgreSQL 13+
- ✅ Relatórios: Power BI

### Métricas de Sucesso

Os modelos devem alcançar:

```
Acurácia:  > 95%
Precisão:  > 94%
Recall:    > 96%
F1-Score:  > 95%
```

Estas métricas devem ser verificadas em TODOS os commits da camada ML.

### Testes Obrigatórios

- [ ] Unit tests (80%+ cobertura)
- [ ] Integration tests
- [ ] Security tests (OWASP Top 10)
- [ ] Performance tests

---

## 🚫 O Que NÃO Pode Ser Feito

### Proibido

1. ❌ Alterar lista de autores/contribuidores original
2. ❌ Adicionar assinatura pessoal/username em arquivos de código
3. ❌ Modificar LICENSE ou copyright
4. ❌ Usar em fins comerciais sem licença apropriada
5. ❌ Remover referências ao artigo ou pesquisa original
6. ❌ Fazer deploy em produção sem aprovação explícita
7. ❌ Expor credenciais ou dados sensíveis no repositório
8. ❌ Violar LGPD ou privacidade dos dados

---

## ✅ Padrões de Código

### Estrutura de Pastas

Respeite a estrutura definida em PLANO_DESENVOLVIMENTO.md

### Nomenclatura

**C# (Backend):**
```csharp
// Classes: PascalCase
public class EmailClassificationService { }

// Métodos: PascalCase
public async Task<ClassificationResult> ClassifyEmailAsync(Email email) { }

// Variáveis: camelCase
var emailContent = email.Body;
```

**Python (ML):**
```python
# Funções: snake_case
def classify_email(email_text):
    pass

# Classes: PascalCase
class NaiveBayesClassifier:
    pass

# Constantes: UPPER_SNAKE_CASE
MIN_CONFIDENCE = 0.5
```

**JavaScript (Frontend):**
```javascript
// Componentes: PascalCase
export const EmailAlertComponent = () => { };

// Funções: camelCase
const handleEmailClassification = () => { };

// Constantes: UPPER_SNAKE_CASE
const API_ENDPOINT = 'http://localhost:5001/api';
```

### Documentação

Todos os arquivos devem incluir:

```
/**
 * Descrição breve do arquivo/função
 * 
 * Descrição detalhada se necessário
 * 
 * @author Equipe do Projeto
 * @version 1.0
 * @date 30/08/2026
 */
```

---

## 🔐 Segurança

### Checklist de Segurança para PRs

- [ ] Nenhuma credencial commitada
- [ ] Senhas usando hash/vault
- [ ] Inputs validados
- [ ] SQL Injection prevention
- [ ] CORS configurado corretamente
- [ ] Logs não expõem dados sensíveis
- [ ] Certificados SSL configurados

---

## 📚 Documentação Obrigatória

Ao adicionar funcionalidade, atualize:

1. `PLANO_DESENVOLVIMENTO.md` - Se mudar roadmap
2. `README.md` - Se mudar instalação/uso
3. `docs/ARQUITETURA.md` - Se mudar arquitetura
4. `docs/API_SPEC.md` - Se adicionar endpoints
5. Código comentado em inglês ou português consistente

---

## 🧪 Processo de Review

### Antes de Merge

1. **Code Review**
   - Pelo menos 1 membro autorizado
   - Verificar aderência aos padrões
   - Testar localmente

2. **Testes**
   - CI/CD deve passar
   - Cobertura mínima: 80%
   - Performance tests: OK

3. **Documentação**
   - README atualizado?
   - Código comentado?
   - Changelog atualizado?

### Exemplo de Changelog

```markdown
## [1.0.0] - 2026-08-30

### Added
- Classificação com Naive Bayes + Random Forest
- Dashboard interativo Power BI
- Alertas em tempo real

### Fixed
- Bug na integração IMAP

### Changed
- Melhorado performance do modelo ML
```

---

## 📞 Contato

Para questões sobre contribuições:
- Contactar Prof. Me. Gilberto Alves Pereira
- Ou membro autorizado do time original

---

## 📄 Licença

Este projeto é confidencial para Health2B Hospitalar Ltda.

Todos os contribuidores concordam em:
- ✅ Respeitar a autoria original
- ✅ Não adicionar-se como autor
- ✅ Seguir as regras de confidencialidade
- ✅ Usar apenas para fins aprovados

---

**Última atualização:** 30 de Agosto de 2026
