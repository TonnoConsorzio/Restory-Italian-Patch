PATCH ITALIANA PER RESTORY

INSTALLAZIONE
1. Chiudi Restory.
2. Fai doppio clic su install.cmd: cerca la build supportata nelle librerie Steam e usa automaticamente l'EXE più recente in dist.
3. Puoi anche trascinare la cartella Restory su install.cmd oppure usare:
   RestoryItalianPatch.exe install "D:\percorso\Restory"
4. Avvia il gioco e seleziona Italiano nelle impostazioni della lingua.

RIMOZIONE
Fai doppio clic su uninstall.cmd e inserisci il percorso se richiesto, oppure esegui:
   RestoryItalianPatch.exe uninstall "D:\percorso\steamapps\common\Restory"
Il patcher rimuove i file che ha installato e ripristina quelli sostituiti.

REQUISITI
- Build stabile verificata: Restory 1.0.035r (build 26622427), con 2.187 chiavi UI tradotte e la nuova entry dialogo 156/13.
- Resta supportata anche Restory 1.0.033r (build 26253677), con 2.146 chiavi UI.
- Il pacchetto contiene il loader necessario; non serve scaricare file separati.
- Non installare o disinstallare mentre il gioco e' aperto.
- Un'altra build viene rifiutata prima di modificare i file.
- Il ramo beta Steam [beta_version] non e' supportato.

Se hai installato BepInEx o altri mod, il patcher conserva i file estranei alla patch.
Vedi THIRD-PARTY-NOTICES.md per le licenze del loader incluso.

TEST E COMPILAZIONE
Da PowerShell, con i riferimenti gestiti del gioco:
   .\test.ps1 -ManagedGameFolder "D:\percorso\Restory_Data\Managed"
   .\build.ps1 -ManagedGameFolder "D:\percorso\Restory_Data\Managed"
I test CMD e CLI installano e rimuovono la patch solo su fixture temporanee per entrambi i BuildID; non avviano ne' modificano il gioco installato.

Il controllo automatico delle 41 nuove chiavi UI non richiede il corpus originale. Il controllo completo contro un export JSON locale e' facoltativo:
   .\tests\FuzzbyUiCoverage.Tests.ps1 -SourceCorpus "C:\percorso-locale\ui-source.json"
Non aggiungere al repository export o copie dei testi originali del gioco.
