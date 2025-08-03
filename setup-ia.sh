#!/bin/bash

echo "🚀 Configurando IA Local para CappyAI..."

# Verificar se Docker está instalado
if ! command -v docker &> /dev/null; then
    echo "❌ Docker não está instalado. Por favor, instale o Docker primeiro."
    exit 1
fi

# Verificar se Docker Compose está instalado
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose não está instalado. Por favor, instale o Docker Compose primeiro."
    exit 1
fi

echo "✅ Docker e Docker Compose encontrados"

# Parar containers existentes
echo "🛑 Parando containers existentes..."
docker-compose down

# Construir e iniciar os serviços
echo "🔨 Construindo e iniciando serviços..."
docker-compose up -d --build

# Aguardar o Ollama inicializar
echo "⏳ Aguardando Ollama inicializar..."
sleep 10

# Baixar modelo Llama2 (ou outro modelo de sua preferência)
echo "📥 Baixando modelo Llama2..."
docker exec ollama ollama pull llama2

echo "✅ Setup concluído!"
echo ""
echo "🌐 API disponível em: http://localhost:7001"
echo "📚 Swagger em: http://localhost:7001/swagger"
echo "🤖 Ollama em: http://localhost:11434"
echo ""
echo "Para testar a API:"
echo "curl -X POST http://localhost:7001/api/QuebraGelo/gerar-automatico \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"quantidade\": 3}'"
echo ""
echo "Para parar os serviços: docker-compose down" 