"""Serve the trained Fairy model with a readiness endpoint for DSH startup."""
from pathlib import Path
import os,sys

# DSH adds IPv6 loopback patterns to NO_PROXY. The bundled older httpx
# cannot parse those patterns at Gradio import time. Keep this local-only
# service's loopback bypass explicit without changing DSH or system settings.
os.environ['NO_PROXY']='localhost,127.0.0.1'
os.environ['no_proxy']='localhost,127.0.0.1'

ROOT=Path(__file__).resolve().parent
SRC=Path(os.environ.get('FAIRY_RUNTIME_SOURCE', str(ROOT/'runtime-src')))
os.chdir(SRC)
sys.path[:0]=[str(SRC),str(SRC/'GPT_SoVITS')]
import api_v2

@api_v2.APP.get('/fairy/health')
async def fairy_health():
    config=api_v2.tts_pipeline.configs
    return {'service':'dsh-fairy-tts','ready':True,'version':config.version,
            'gpt_weights':str(Path(config.t2s_weights_path).resolve()),
            'sovits_weights':str(Path(config.vits_weights_path).resolve()),
            'pid':os.getpid()}

if __name__=='__main__':
    api_v2.uvicorn.run(api_v2.APP,host=api_v2.host,port=api_v2.port,workers=1)
