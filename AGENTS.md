# Repository guidance

## Project overview

SyllableSplitter is a C# console application that splits words into syllables using configurable language rules and collects consonant-cluster statistics. The solution contains two SDK-style projects targeting **.NET Framework 4.8 (`net48`)**; it is not a modern .NET runtime application.

## Repository map

- `SyllableSplitter.sln`: application and test projects, with Debug/Release Any CPU configurations.
- `SyllableSplitter/Program.cs`: CLI arguments, lowercasing and deduplicating words, trace output, and cluster-report files.
- `SyllableSplitter/Configuration.cs`: JSON configuration fields and Newtonsoft.Json deserialization.
- `SyllableSplitter/SyllableBreaker.cs`: letter-class expansion, rewrite rules, prefix handling, tokenization, syllable boundaries, and cluster collection.
- `SyllableSplitter/Syllable.cs`, `LetterCluster.cs`, and `RewriteRule.cs`: supporting models.
- `SyllableSplitter/WordReader.cs`: file/stdin word extraction and joining lines ending in a hyphen.
- `SyllableSplitter/Extensions/IEnumerableExtensions.cs`: parsing by the first matching item in enumeration order.
- `SyllableSplitter/Config/`: German rules and two separate Ukrainian rule sets.
- `SyllableSplitter/Text/`: sample German and Ukrainian input corpora.
- `SyllableSplitterUnitTests/`: NUnit fixtures, shared `RuleTestBase`, and embedded CSV cases in `Data/`.
- `.github/workflows/github-actions-demo.yml`: demonstration workflow that lists files; it does not build or run tests.

## Build and validation

Use Windows with a .NET SDK that can build the SDK-style projects, .NET Framework 4.8 reference assemblies (Developer/Targeting Pack), and the .NET Framework runtime for executing tests and the application. No SDK version is pinned in `global.json`.

Run these commands from the repository root:

```powershell
dotnet restore SyllableSplitter.sln
dotnet build SyllableSplitter.sln --no-restore -m:1
dotnet test SyllableSplitter.sln --no-restore -m:1 --verbosity minimal
```

`-m:1` keeps MSBuild execution serial and is useful in restricted environments. After dependencies are restored, use `--no-restore` to avoid unnecessary network access. For Release validation, add `--configuration Release` to both build and test commands.

To run a single Ukrainian fixture:

```powershell
dotnet test SyllableSplitterUnitTests/SyllableSplitterUnitTests.csproj --no-restore -m:1 --filter "FullyQualifiedName~SyllableSplitterUnitTests.Ukrainian2Test"
```

For engine changes, run both Ukrainian fixtures. A successful demonstration workflow is not evidence that the solution builds or its tests pass. Report actual validation results and distinguish tool/environment failures from assertion failures.

Dependencies are managed through `PackageReference` in the project files: Newtonsoft.Json in the application; Microsoft.NET.Test.Sdk, NUnit 3, and NUnit3TestAdapter in the tests. The local `packages/` directory is not the source of dependency declarations.

## Running the CLI

After a Debug build, a sample invocation from the repository root is:

```powershell
& .\SyllableSplitter\bin\Debug\net48\SyllableSplitter.exe .\SyllableSplitter\Config\ukrainian2.json .\SyllableSplitter\Text\ua_iliad.txt
```

The first argument is the config path; the optional second argument is a text file. Omitting the text file reads stdin. The CLI lowercases words and processes each distinct word once. It prints `word=syll-a-bles` and currently enables tracing, so stdout also contains diagnostic lines.

Each completed run writes or overwrites `clusters_by_count.txt` and `clusters_by_length.txt` in the **current working directory**. For smoke tests, use a temporary working directory and absolute paths to the executable, configuration, and input. Do not commit these generated reports.

## Language-rule constraints

- Prefer config changes for language-specific behavior; change the engine when the requested behavior cannot be represented by the existing rules.
- Preserve list order. Letter tokenization takes the first matching alphabet entry, prefix handling takes the first matching prefix, rewrite rules execute sequentially, and the first matching split rule wins. Multi-character letters such as `дзь`, `дж`, and `sch` must precede shorter entries that could consume their beginning.
- `LetterClasses` entries use `name=item,item` (for example, `<гол>=а,е,...`). `Vowels` and `Consonants` are comma-separated tokens and may reference classes. `Prefixes` is comma-separated; `Separators` is a string of individual separator characters.
- Rewrite rules use `search/replacement` or `search/replacement/context`; `_` marks the replacement position in a context. Class-to-class replacement maps entries by index, so preserve the correspondence and provide a replacement for each searchable entry.
- Split rules use `coda/onset`. Patterns are expanded to regexes over bracketed letter tokens, and `.` represents one token, not necessarily one character. Keep specific rules before general fallbacks.
- `BreakWord` separates the input at configured separators, rewrites each part, splits prefixes recursively, then constructs syllables and redistributes consonants according to split rules. The CLI lowercases input; `BreakWord` itself does not.
- Keep `ukrainian.json` and `ukrainian2.json` distinct: the first has prefix splitting; the second has no configured prefixes and uses consonant classes for boundary rules.
- Preserve UTF-8 text, Cyrillic filenames, apostrophes, soft-sign combinations, and German characters. Do not normalize or transliterate fixtures or corpora incidentally.
- Use the existing Newtonsoft.Json loader when checking compatibility. `ukrainian2.json` currently contains a trailing comma and a `LettersInSyllables` property absent from `Configuration`; avoid introducing strict parsing or assigning behavior to that property incidentally.

## Tests and regression cases

- Use NUnit 3 conventions already present: `[TestFixture]`, `[Test]`, `[TestCaseSource]`, and `Assert.AreEqual`.
- `RuleTestBase` loads configurations from `Config/` beside the test assembly. Application configs are copied to build output; CSV files are embedded resources via `Data\*.csv`. Preserve those project settings.
- CSV rows have no header. The loader splits on commas, trims the first two fields, and uses them as `word, expected-hyphenated-result`; remaining fields are annotations. It is not a quoted CSV parser.
- Add language regression cases to the dataset used by the relevant fixture. Adding a CSV only embeds it; wire it into a `TestCaseSource` to execute it. In particular, `Data/Ukrainian2.csv` is currently not referenced by a test case source.
- For behavior changes, add focused cases showing the requested boundary or rewrite and nearby cases that should retain their behavior. Do not adjust expected results merely to make failing tests pass.
- There is no German NUnit fixture currently. For German-rule changes, add suitable coverage or explicitly report the limits of a CLI smoke test.

## Coding and change scope

- Follow the existing C# style: four-space indentation, braces on their own lines, block namespaces, PascalCase types/methods/public members, and camelCase parameters/private fields. Keep additions compatible with the default C# 7.3 language level for this target.
- Preserve existing public configuration fields and JSON names unless a schema/API change is requested. Avoid unrelated field-to-property conversions, formatting sweeps, and dependency upgrades.
- Both projects set `GenerateAssemblyInfo=false`; assembly metadata is maintained in `Properties/AssemblyInfo.cs`.
- Do not retarget the projects or migrate test frameworks as part of unrelated work. SDK-style source inclusion means ordinary new `.cs` files do not require explicit `Compile` entries.
- Keep changes focused. Do not modify sample corpora or the tracked empty file `SyllableSplitter/a` without a task-specific reason.
- Do not commit `bin/`, `obj/`, `.vs/`, restored packages, test outputs, or cluster reports. Check the final diff for unintended files, and update `README.md` when CLI usage or observable behavior changes.
