// Explicit local-only import. Never export a private corpus into public assets.
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
if (!process.argv[2]) throw new Error('Usage: node tools/sync-persona.cjs <Fairy-DSH/fairy-contracts/persona.cjs>');
const source = path.resolve(process.argv[2]);
const persona = require(source);
const root = path.resolve(__dirname, '..');
const prompt = persona.createPersonaPrompt(persona.DEFAULT_PERSONA) + '\n\n' + persona.createPersonaReminder(persona.DEFAULT_PERSONA);
const local = path.join(root, 'runtime/private');
fs.mkdirSync(local, { recursive: true });
fs.writeFileSync(path.join(local, 'fairy-persona.txt'), prompt, 'utf8');
fs.writeFileSync(path.join(local, 'fairy-persona-source.local.json'), JSON.stringify({source:'Fairy-DSH/fairy-contracts/persona.cjs',settings:persona.DEFAULT_PERSONA,sha256:crypto.createHash('sha256').update(prompt).digest('hex')}, null, 2));
console.log('Imported local-only persona; ignored by Git. Public fallback unchanged.');
