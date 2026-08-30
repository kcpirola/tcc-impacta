"""Configuração do pytest: garante que o diretório ml/ esteja no sys.path."""
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
