# syntax=docker/dockerfile:1
# =====================================================================
# Advanced Claim Analytics — LangChain sidecar (FastAPI) container image.
# Build context = this directory:  docker build -t <tag> ai-sidecars/langchain-claim-analytics
# Optional advisory layer over the .NET RAG core. Deterministic by default
# (no API key, no paid provider, no network). Optional local ChatOllama only
# when OLLAMA_BASE_URL is set. No secrets are baked in. Runs non-root on :8090.
# =====================================================================
FROM python:3.12-slim AS final
WORKDIR /app

ENV PYTHONUNBUFFERED=1 \
    PYTHONDONTWRITEBYTECODE=1 \
    PIP_NO_CACHE_DIR=1

# Install deps first so the layer caches unless requirements change.
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY app.py .

# Run as a non-root user.
RUN useradd --uid 1654 --create-home appuser
USER appuser

EXPOSE 8090
CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8090"]
