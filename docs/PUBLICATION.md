# Pubblicazione

Il repository locale segue `main` e ha il remote `origin` configurato su GitHub. Le modifiche locali possono essere inviate con un commit incrementale; non riscrivere la cronologia esistente.

## Contenuto del repository

- Non aggiungere `work/`, `dist/`, `game-copy/`, `.worktrees/`, estrazioni del gioco o copie dei file originali di Restory.
- Il payload contiene i cataloghi italiani e i file del loader necessari all'installazione. I file di terze parti sono elencati in `THIRD-PARTY-NOTICES.md`.
- La licenza MIT copre soltanto codice e strumenti originali. I cataloghi di traduzione e i componenti di terze parti non sono concessi sotto MIT.

## Prima di distribuire una release

1. Verificare i termini di redistribuzione del loader e dei binari inclusi nel payload; mantenere gli avvisi di `THIRD-PARTY-NOTICES.md`.
2. Chiarire i diritti di redistribuzione dei testi tradotti derivati dal gioco. La pubblicazione del codice non concede diritti sui testi, contenuti o marchi di Restory.
3. Verificare nel gioco la selezione di una risposta tradotta, le etichette dei personaggi e il layout alle risoluzioni supportate. Il credito della home è stato controllato visivamente dopo il riavvio.
4. Generare il pacchetto con `build.ps1` solo dopo aver verificato che la build Steam supportata sia installata. Gli artefatti generati sotto `dist/` restano esclusi dal repository; pubblicarli come release solo dopo i controlli sopra.

## Identità Git

I commit usano l'indirizzo `alessiobellan@ik.me`, come richiesto dall'autore.
