# Patch italiana per Restory

Traduzione amatoriale in italiano per **Restory** su Windows. La patch aggiunge la lingua italiana alle opzioni del gioco e traduce interfaccia, obiettivi, dialoghi e risposte.

> Progetto non ufficiale, non affiliato né approvato dagli sviluppatori di Restory o da Steam. Richiede una copia legittima del gioco.

## Compatibilità

- Gioco: Restory per Windows, Steam
- Build stabile verificata: **26622427** (versione **1.0.035r**)
- Build precedente ancora supportata: **26253677** (versione **1.0.033r**)
- Interfaccia della build stabile: **2.187** chiavi tradotte, incluse le **41** aggiunte con Fuzzby
- Interfaccia della build precedente: **2.146** chiavi tradotte
- Dialoghi e risposte: **1.132** chiavi runtime, inclusa la nuova entry **156/13**
- Il testo originale del gioco non è incluso nel repository.

L'installer controlla la build prima di modificare i file e si ferma se la cartella selezionata non corrisponde a una versione supportata.

## Installazione

1. Chiudi Restory.
2. Scarica l'archivio di una release GitHub e decomprimilo. Se non è ancora disponibile una release, compila il progetto seguendo la sezione [Compilare dai sorgenti](#compilare-dai-sorgenti).
3. Avvia `install.cmd`. Lo script cerca automaticamente Restory nelle librerie Steam; se non trova la cartella corretta, puoi indicarla quando richiesto. In alternativa, trascina la cartella del gioco su `install.cmd`.
4. Avvia Restory e seleziona **Italiano** nelle impostazioni della lingua.

La patch include il loader necessario: non serve scaricarlo a parte. L'installer conserva i file preesistenti e crea backup per consentire il ripristino. La beta Steam `[beta_version]` e le sue modifiche di progressione non sono supportate.

### Disinstallazione

Chiudi il gioco e avvia `uninstall.cmd`. Se necessario, indica la cartella di Restory. Il programma rimuove i file della patch e ripristina i file salvati durante l'installazione; non sovrascrive file della patch modificati dopo l'installazione.

Per usare direttamente il programma da CMD:

```cmd
RestoryItalianPatch.exe install "D:\Steam\steamapps\common\Restory"
RestoryItalianPatch.exe uninstall "D:\Steam\steamapps\common\Restory"
```

## Crediti nel gioco

La schermata iniziale mostra: **Tradotto con ❤️ e 🍕 da Alessio** e **IG alessio.bellan**.

## Compilare dai sorgenti

Servono Windows, il compilatore C# incluso nel .NET Framework e i riferimenti gestiti del gioco. In PowerShell, dalla cartella del repository:

```powershell
$managed = 'D:\Steam\steamapps\common\Restory\Restory_Data\Managed'
.\test.ps1 -ManagedGameFolder $managed
.\build.ps1 -ManagedGameFolder $managed
```

La build crea un pacchetto con data e ora in `dist\RestoryItalianPatch-<timestamp>`. In alternativa, imposta `RESTORY_MANAGED_FOLDER` sul percorso `Restory_Data\Managed`. I test automatici usano fixture e copie temporanee per verificare installazione/rimozione CMD e CLI per entrambi i BuildID supportati; non avviano né modificano il gioco installato.

## Traduzioni e contributi

I cataloghi italiani sono file TSV sotto `payload/`. Quando modifichi una voce, conserva l'identificativo, i placeholder (`{0}` e simili), i tag e la formattazione. Non aggiungere al repository estrazioni o copie dei file originali del gioco.

Per verificare una modifica, compilare i plugin ed eseguire il controllo integrato delle 41 chiavi Fuzzby senza estratti del gioco:

```powershell
.\test.ps1 -ManagedGameFolder $managed
```

Il controllo completo della copertura UI rispetto all'intero corpus resta facoltativo e richiede un percorso locale a un export JSON in sola lettura, che non va aggiunto al repository:

```powershell
.\tests\FuzzbyUiCoverage.Tests.ps1 -SourceCorpus 'C:\percorso-locale\ui-source.json'
```

## Licenza e diritti

La licenza [MIT](LICENSE) copre solo il codice e gli strumenti originali di questo progetto. Non copre i cataloghi di traduzione, i file del loader di terze parti o i contenuti e marchi di Restory. Consulta [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) per le dipendenze incluse e le relative note.
