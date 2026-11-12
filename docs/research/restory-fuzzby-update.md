# Ricerca sull’aggiornamento Fuzzby

**Data:** 2026-11-12  
**Build target osservato:** ReStory 1.0.035r, Steam BuildID 26622427  
**Stato:** ispezione locale in sola lettura; il gioco non è stato avviato né modificato.

## Annuncio ufficiale

Il post Steam “The FUZZBY Update is Here!” è datato 27 ottobre 2026. Annuncia Fuzzby e il controller originale Nony PlayMachine, Steam Deck Verified e numerosi miglioramenti e correzioni. La correzione per alcuni problemi di progressione della storia è distribuita separatamente nel ramo opt-in `[beta_version]`.

L’annuncio non fornisce un numero di versione del gioco né un BuildID. Non usare il post come fonte del numero della build: il target `1.0.035r` / `26622427` è stato verificato nella build Steam installata localmente (PlayerSettings e appmanifest).

## Differenze dei dati locali

- La tabella inglese di localizzazione contiene 2.187 chiavi, contro le 2.146 dell’estrazione precedente: **41 chiavi nuove** da tradurre e aggiungere ai controlli di copertura.
- L’export Articy contiene 1.133 `DialogueFragment`, uno in più rispetto ai 1.132 precedenti. Il database runtime mantiene 1.132 chiavi uniche: questo frammento non aggiunge una nuova chiave runtime.
- Il frammento Articy `0x0100000000006FEC` è presente anche nel `DialogueDatabase` della build aggiornata e corrisponde allo slot runtime già esistente conversazione `156`, entry `13`, con sorgente inglese nel database.
- La corrispondenza è stata verificata confrontando l’`Articy Id` della entry runtime con l’ID del nodo Articy. Il testo sorgente non viene copiato in questa nota o nel repository.

## Fonti

- [Annuncio Steam: The FUZZBY Update is Here!](https://store.steampowered.com/news/app/3812600/view/692025835039752633)
- [Feed ufficiale delle notizie Steam dell’app](https://store.steampowered.com/news/posts/?appids=3812600)
- [Notizie ufficiali della Community Steam](https://steamcommunity.com/app/3812600/allnews)

L’URL diretto dell’annuncio non era accessibile allo strumento di ricerca; titolo e contenuto sono stati confrontati con le copie ufficiali Steam del feed e della Community.
