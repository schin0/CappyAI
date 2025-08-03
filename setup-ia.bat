@echo off
echo 🚀 Configurando IA Local para CappyAI...

REM Verificar se Docker está instalado
docker --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker não está instalado. Por favor, instale o Docker primeiro.
    pause
    exit /b 1
)

REM Verificar se Docker Compose está instalado
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker Compose não está instalado. Por favor, instale o Docker Compose primeiro.
    pause
    exit /b 1
)

echo ✅ Docker e Docker Compose encontrados

REM Parar containers existentes
echo 🛑 Parando containers existentes...
docker-compose down

REM Construir e iniciar os serviços
echo 🔨 Construindo e iniciando serviços...
docker-compose up -d --build

REM Aguardar o Ollama inicializar
echo ⏳ Aguardando Ollama inicializar...
timeout /t 10 /nobreak >nul

REM Baixar modelo Llama2
echo 📥 Baixando modelo Llama2...
docker exec ollama ollama pull llama2

echo ✅ Setup concluído!
echo.
echo 🌐 API disponível em: http://localhost:7001
echo 📚 Swagger em: http://localhost:7001/swagger
echo 🤖 Ollama em: http://localhost:11434
echo.
echo Para testar a API, use o Swagger ou faça uma requisição POST para:
echo http://localhost:7001/api/QuebraGelo/gerar-automatico
echo.
echo Para parar os serviços: docker-compose down
pause 