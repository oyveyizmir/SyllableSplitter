# SyllableSplitter

SyllableSplitter is a command-line utility that splits words into syllables using a configurable
set of language rules. You run it with a JSON config (language rules) and optionally a text file,
and it outputs each unique word with its syllable breakdown. It also collects consonant clusters
and writes two summary files.

## How It Works

At startup, the program reads a configuration JSON, builds a `SyllableBreaker`, then reads words
either from a text file or stdin. For each unique word, it lowercases it, splits it into syllables,
and prints `word=syll-a-bles`. After finishing, it writes two files: `clusters_by_count.txt` and
`clusters_by_length.txt` listing consonant clusters and example words.

## Inputs

- Config JSON: defines vowels, consonants, prefixes, separators, letter classes, rewrite rules,
  and split rules.
- Optional text file: words are extracted using a Unicode letter regex; hyphenated line breaks
  are stitched.

Example configs:

- `SyllableSplitter\Config\deutsch.json`
- `SyllableSplitter\Config\ukrainian.json`
- `SyllableSplitter\Config\ukrainian2.json`

## Core Logic (Syllable Splitting)

- A word is first rewritten with configurable rewrite rules (e.g., multi-char letters).
- It is then segmented into "letters" based on the configured alphabet (supports multi-char
  letters via letter classes).
- Syllables are built by assigning consonants to onset/coda around vowel nuclei.
- Split rules (regex over bracketed letter clusters) decide where to cut consonant clusters
  between adjacent syllables.
- Prefix handling: if a word starts with a configured prefix, it splits the prefix separately,
  then continues on the remainder.

## Outputs

- Console lines like: `word=syll-a-bles` for each unique word.
- `clusters_by_count.txt`: consonant clusters sorted by how often they appear.
- `clusters_by_length.txt`: consonant clusters sorted by size/length.

## Usage

Both projects target .NET 10. Install the .NET 10 SDK selected by `global.json`
(10.0.402 or a later patch in the same SDK feature band) to build and test the solution.
The .NET Framework Developer Pack is no longer required.

From the repository root:

```powershell
dotnet restore SyllableSplitter.sln
dotnet build SyllableSplitter.sln --no-restore
dotnet test SyllableSplitter.sln --no-restore
```

Run with a configuration JSON file and an optional text file. Arguments after `--`
are passed to the application. If the text file is omitted, words are read from standard input.

```powershell
dotnet run --project SyllableSplitter -- <ConfigFile> [<TextFile>]
```

Example:

```powershell
dotnet run --project SyllableSplitter -- SyllableSplitter/Config/ukrainian.json SyllableSplitter/Text/ua_iliad.txt
```

The CLI currently enables tracing, so console output also includes diagnostic lines.
The cluster reports are written to the current working directory.

## Publish

Create a framework-dependent Release build with its language configurations and sample texts:

```powershell
dotnet publish SyllableSplitter/SyllableSplitter.csproj --configuration Release --self-contained false --output artifacts/publish
dotnet artifacts/publish/SyllableSplitter.dll artifacts/publish/Config/ukrainian2.json artifacts/publish/Text/ua_iliad.txt
```

The published application requires the .NET 10 runtime. On Windows, it also includes
`SyllableSplitter.exe`; on Linux or macOS, the native launcher is `SyllableSplitter`.
The `dotnet SyllableSplitter.dll` invocation works across platforms when the matching runtime is installed.

The existing GitHub Actions demonstration workflow does not build or test the solution;
run the validation commands above before submitting changes.

THE END