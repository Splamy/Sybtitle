# Sybtitle

Cli tool for subtitle file manipulation.

## Usage

### Offset
Offsets all entries by fixed time

Syntax `--offset <duration:timespan>`

```powershell
Sybtitle.exe --offset 4s
Sybtitle.exe --offset -2s300ms
```

### Rescale
Streches all subtitle entries within a time region

Syntax `--rescale (<src_start:timespan> <src_end:timespan> <dst_start:timespan> <dst_end:timespan>)+ (auto)?`

```powershell
## All Entries in 00:00 to 10:00 will be stretched linear to 00:00 to 20:00
## Ex: 2:00 -> 4:00, 00:10 -> 00:20
Sybtitle.exe --rescale 00:00 10:00 00:00 20:00

## Append 'auto' at the end to offset all entries after the rescale region accordingly
## Ex: will shift 21:00 -> 31:00
Sybtitle.exe --rescale 00:00 10:00 00:00 20:00 auto
```
