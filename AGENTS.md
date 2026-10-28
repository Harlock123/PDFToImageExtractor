# AGENTS.md

Notes for anyone, human or AI agent, changing this code. There is no README; this file says what the tool does and
how to build and change it.

## What this is

A small .NET 9 console tool that renders every page of a PDF to a greyscale PNG on a white background. It uses
Docnet.Core (PDFium) to render and SixLabors.ImageSharp to process and save. Everything is in `Program.cs`.

## Commands

```bash
dotnet build PdfToImageExtractor.csproj           # .NET 9 SDK; no solution file or global.json
dotnet run -- "SampleFiles/COB TEST_PROF (1).pdf"   # one argument: the PDF to convert
```

- Output goes to an `OutputImages` folder beside the input PDF, one file per page, named
  `<pdf name>_PAGE001.png`, `_PAGE002.png` and so on. Existing files are overwritten.
- There are no tests yet, and no CI workflow.

## Layout

- `Program.cs`: argument check, then for each page:
  - render at the `PageDimensions(1080, 1920)` bounds;
  - swap the BGRA bytes Docnet returns into RGBA;
  - apply `Grayscale()`;
  - flatten onto white and save as PNG.
- `SampleFiles/`: two sample PDFs to try it on, and a Word document of example output images.

## Rules that keep it working

- **Pixel order.** Docnet's `GetImage()` returns BGRA; ImageSharp is loaded as `Rgba32`. Keep the swap if either
  side changes.
- **Flatten onto white.** PDFium leaves the page background transparent, so drawing onto a white image is what makes
  the PNGs readable. A commented-out `BinaryThreshold` line is there for black-and-white output.
- **Unused package.** `PDFium.NET` is referenced but not used; Docnet.Core brings its own PDFium native libraries.
