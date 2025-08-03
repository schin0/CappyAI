# Feature: Quebra-Gelo 🧊

## Sobre a Feature

A feature de **Quebra-Gelo** foi desenvolvida para ajudar pessoas a iniciarem conversas de forma natural e divertida em eventos presenciais. O objetivo é motivar a saída do celular e promover conexões reais entre as pessoas.

## Funcionalidades

### 🎯 Geração Inteligente de Ideias
- **Contexto Automático**: Usa localização, clima, horário e interesses do usuário
- **Personalização Cultural**: Ideias adaptadas à cultura regional
- **Filtros por Interesses**: Sugestões baseadas nos interesses dos participantes
- **Níveis de Dificuldade**: Desde ideias simples até desafios mais elaborados
- **Tipos Variados**: Perguntas, jogos, desafios, temas de conversa e atividades interativas
- **🤖 IA Local**: Geração de ideias personalizadas usando IA local (Ollama)

### 📱 API RESTful
- **Endpoint Principal**: `POST /api/QuebraGelo/gerar-automatico`
- **Contexto Automático**: `GET /api/QuebraGelo/contexto-atual`
- **Documentação Swagger**: Disponível em `/swagger`
- **Exemplos Práticos**: Endpoint `/api/QuebraGelo/exemplo`

## 🤖 IA Local com Docker

### Setup Rápido

#### Windows:
```bash
setup-ia.bat
```

#### Linux/Mac:
```bash
chmod +x setup-ia.sh
./setup-ia.sh
```

#### Manual:
```bash
# 1. Construir e iniciar serviços
docker-compose up -d --build

# 2. Baixar modelo de IA
docker exec ollama ollama pull llama2

# 3. Testar
curl -X POST http://localhost:7001/api/QuebraGelo/gerar-automatico \
  -H 'Content-Type: application/json' \
  -d '{"quantidade": 3}'
```

### Configuração da IA

A aplicação usa **Ollama** para IA local, que oferece:

- **Privacidade Total**: Tudo roda localmente
- **Sem Custos**: Sem APIs pagas
- **Modelos Variados**: Llama2, Mistral, CodeLlama, etc.
- **Customização**: Prompts específicos para quebra-gelo

### Modelos Disponíveis

Para trocar o modelo, edite `appsettings.json`:

```json
{
  "IA": {
    "Url": "http://localhost:11434/api/generate",
    "Modelo": "llama2"  // ou "mistral", "codellama", etc.
  }
}
```

### Fallback Inteligente

Se a IA não estiver disponível, o sistema automaticamente usa as ideias pré-definidas, garantindo que sempre funcione.

## Tipos de Quebra-Gelo

1. **Pergunta** - Questões criativas e interessantes
2. **Jogo** - Atividades lúdicas para grupos
3. **Desafio** - Tarefas rápidas e divertidas
4. **TemaConversa** - Assuntos para discussão
5. **AtividadeInterativa** - Exercícios que envolvem movimento

## Contexto Automático

A feature agora considera automaticamente:

### 🌍 **Localização e Cultura**
- **São Paulo**: Ideias sobre metrô, bairros, cultura paulistana
- **Rio de Janeiro**: Temas sobre praias, cultura carioca
- **Minas Gerais**: Conversas sobre café, cultura mineira
- **Outras regiões**: Adaptações culturais específicas

### 🌤️ **Clima e Estação**
- **Ensolarado**: Atividades ao ar livre, energia positiva
- **Chuvoso**: Conversas aconchegantes, atividades internas
- **Frio**: Temas quentes, conforto
- **Quente**: Atividades refrescantes, energia

### ⏰ **Horário do Dia**
- **Manhã**: Energia, rotinas, planos do dia
- **Tarde**: Ritual de recarga, atividades
- **Noite**: Planos, diversão, relaxamento
- **Madrugada**: Conversas profundas, reflexões

## Como Usar

### Exemplo Básico (Recomendado)
```json
POST /api/QuebraGelo/gerar-automatico
{
  "quantidade": 3,
  "tipoPreferido": null,
  "nivelDificuldadeMaximo": 2
}
```

### Exemplo com Contexto Personalizado
```json
POST /api/QuebraGelo/gerar
{
  "contexto": {
    "localizacao": "São Paulo, SP",
    "climaAtual": "ensolarado",
    "horaAtual": 14,
    "diaSemana": "quinta",
    "estacaoAno": "primavera",
    "interessesUsuario": ["tecnologia", "música", "viagem"],
    "culturaLocal": "paulista"
  },
  "quantidade": 5,
  "tipoPreferido": "Pergunta",
  "nivelDificuldadeMaximo": 2
}
```

## Exemplos de Ideias Geradas

### Para São Paulo (Manhã Ensolarada)
- "Como você gosta de começar suas manhãs?"
- "Qual é seu bairro favorito em São Paulo e por quê?"
- "Qual seria a atividade perfeita para o clima de hoje?"

### Para Rio de Janeiro (Tarde de Verão)
- "Qual é sua praia preferida no Rio e o que você mais gosta dela?"
- "Qual é seu ritual preferido para recarregar as energias à tarde?"
- "Qual seria a atividade perfeita para o clima de hoje?"

### Para Belo Horizonte (Noite Fria)
- "Qual é seu café da manhã mineiro preferido?"
- "Se você pudesse fazer qualquer coisa esta noite, o que seria?"
- "Como o clima de hoje afeta seu humor?"

## Benefícios

### Para os Usuários
- **Redução da Ansiedade Social**: Ideias prontas para iniciar conversas
- **Conexões Mais Autênticas**: Conversas baseadas em interesses reais
- **Experiência Divertida**: Atividades lúdicas que quebram o gelo naturalmente
- **Personalização**: Ideias adaptadas ao momento e local
- **🤖 Criatividade IA**: Ideias únicas e contextualizadas

### Para o App
- **Diferencial Competitivo**: Feature única no mercado
- **Engajamento**: Usuários mais ativos em eventos
- **Retenção**: Experiência positiva incentiva o uso contínuo
- **Contextualização**: Ideias sempre relevantes ao momento
- **Privacidade**: IA local sem dependência de serviços externos

## Próximos Passos

### Melhorias Futuras
1. **IA Generativa**: ✅ Implementado com Ollama
2. **Machine Learning**: Aprendizado com feedback dos usuários
3. **Gamificação**: Sistema de pontos e conquistas
4. **Compartilhamento**: Usuários podem compartilhar ideias favoritas
5. **Multilíngue**: Suporte para diferentes idiomas
6. **API de Clima Real**: Integração com serviços meteorológicos
7. **Modelos Especializados**: Fine-tuning para quebra-gelo

### Integrações
- **Sistema de Perfil**: Interesses baseados no histórico do usuário
- **Geolocalização**: Localização automática via GPS
- **Tempo Real**: Sugestões dinâmicas durante o evento
- **Calendário**: Considerar eventos próximos na agenda

## Tecnologia

- **.NET 8**: Framework principal
- **C# Records**: Modelos imutáveis
- **Dependency Injection**: Arquitetura limpa
- **Swagger**: Documentação automática da API
- **Object Calisthenics**: Princípios de código limpo aplicados
- **HttpClient**: Integração com APIs externas
- **🤖 Ollama**: IA local para geração de ideias
- **Docker**: Containerização completa
- **Docker Compose**: Orquestração de serviços

## Contribuição

A feature foi desenvolvida seguindo os princípios de Object Calisthenics:
- Métodos em português
- Ifs invertidos quando possível
- Código limpo e legível
- Responsabilidades bem definidas 