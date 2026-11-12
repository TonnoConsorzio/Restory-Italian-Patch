# Supporto all’aggiornamento Fuzzby

## Problem Statement

La patch italiana supporta Restory 1.0.033r (Steam BuildID 26253677), ma la copia installata e aggiornata è Restory 1.0.035r (BuildID 26622427). L’installer rifiuta quindi la build corrente e i cataloghi non coprono i nuovi testi introdotti dall’aggiornamento Fuzzby.

## Solution

Aggiornare installer, plugin, dati di traduzione, test e documentazione per la build stabile 1.0.035r / 26622427. La patch resta installabile e rimovibile da CMD, conserva i backup e traduce le nuove chiavi UI e il nuovo dialogo runtime senza alterare gli ID o i collegamenti delle risposte.

## User Stories

1. Come giocatore della build aggiornata, voglio installare la patch da CMD, così posso usare la traduzione italiana anche dopo l’aggiornamento di Restory.
2. Come giocatore, voglio che l’installer riconosca la build stabile 26622427 prima di scrivere file, così evito modifiche a build non verificate.
3. Come giocatore, voglio continuare a installare la patch sulla versione già supportata, così un aggiornamento del patcher non mi blocca se non ho ancora aggiornato Steam.
4. Come giocatore, voglio vedere tradotte le nuove etichette, descrizioni, elementi e email introdotti nell’aggiornamento, così i nuovi contenuti restano comprensibili.
5. Come giocatore, voglio vedere tradotto il nuovo dialogo runtime, così posso seguire la battuta del personaggio nella nuova build.
6. Come giocatore, voglio che la selezione delle risposte mantenga la destinazione originale, così la traduzione non cambia il ramo narrativo scelto.
7. Come giocatore, voglio che testi non coperti continuino a mostrare la lingua originale, così la partita resta utilizzabile anche quando manca una traduzione.
8. Come giocatore, voglio poter reinstallare la patch senza duplicare file e disinstallarla ripristinando i backup, così posso aggiornare o rimuovere la patch in sicurezza.
9. Come contributore, voglio che i test segnalino nuove chiavi UI e nuove entry runtime scoperte nelle sorgenti, così una futura build non riduce silenziosamente la copertura.
10. Come autore, voglio che README, guida CMD e documentazione tecnica dichiarino chiaramente versione/build supportate, così gli utenti scelgono il pacchetto corretto.

## Implementation Decisions

- La build stabile 1.0.035r / Steam BuildID 26622427 è il nuovo target verificato. Mantenere compatibilità con 1.0.033r / 26253677 se i test dimostrano che i due hook e il payload funzionano su entrambe.
- La correzione di progressione della storia nel ramo opt-in `[beta_version]` resta fuori ambito; il target è la build stabile Default.
- La nuova sorgente UI contiene 2.187 chiavi invece di 2.146: aggiungere traduzioni per tutte le 41 chiavi nuove e verificarne placeholder, tag, newline e copertura.
- La nuova sorgente narrativa contiene un ulteriore frammento collegato al runtime come conversazione 156, entry 13. Aggiungerne la traduzione alla mappa runtime verificata, conservando la destinazione della risposta esistente.
- L’installer deve validare il BuildID ammesso prima di creare backup o modificare file. L’installazione e la rimozione restano basate sul manifest reversibile definito da ADR 0001.
- Non avviare né controllare il gioco durante l’esecuzione del lavoro. Verificare automaticamente sorgenti e installer su copie isolate; la verifica visiva in gioco resta manuale e successiva.

## Testing Decisions

- I test coprono il comportamento esterno: riconoscimento delle build ammesse/rifiutate, assenza di modifiche su una build non supportata, installazione ripetuta, ripristino in disinstallazione e avvio CLI.
- I controlli dei cataloghi confrontano gli ID UI con la sorgente della build 1.0.035r, richiedono la copertura delle 41 nuove chiavi e verificano gli ID runtime della nuova entry (156, 13).
- I test di traduzione verificano placeholder, token, markup, newline e fallback; i test della mappa dialoghi controllano la stabilità di conversationId/entryId e preservano la destinazione delle risposte.
- Le verifiche si estendono ai moduli installer, catalogo UI e runtime dialoghi, usando i test CLI, di catalogo e di export già presenti.
- La prova visiva e la selezione effettiva di una risposta in gioco non vengono automatizzate né eseguite mentre il giocatore sta usando il PC.

## Out of Scope

- Installare o tradurre la beta `[beta_version]` e le sue modifiche narrative specifiche.
- Tradurre il testo della notizia Steam o contenuti esterni al gioco.
- Modificare salvataggi, regole di gioco o il gioco installato durante lo sviluppo.
- Pubblicare un nuovo binario di release prima di aver completato le verifiche previste.

## Further Notes

- L’annuncio ufficiale del 27 ottobre 2026 non indica versione o BuildID. Il target 1.0.035r / 26622427 proviene dalla PlayerSettings e dall’appmanifest della build installata.
- L’estrazione comparativa in sola lettura ha trovato 41 nuove chiavi UI e un nuovo frammento Articy con mapping runtime 156/13. I testi originali estratti restano fuori dal repository.
- Evidenze e fonti: `docs/research/restory-fuzzby-update.md`.
- Seam end-to-end approvato: installa/rimuovi da CMD su copia isolata della build stabile, seleziona Italiano, verifica UI e dialoghi; niente controllo del PC o riavvio del gioco in uso.
