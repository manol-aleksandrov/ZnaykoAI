---
name: znayko-skill
description: Enforces Znayko AI coding standards for Razor Pages, EF Core, A4 quiz print layouts, and LLM JSON integration. Use when building or modifying Znayko AI features, PageModels, services, EF queries, quiz/test sheets, or Generative AI code.
---

# Znayko Skill

Coding standards for **Znayko AI** (ASP.NET Core Razor Pages, EF Core + SQLite, Tailwind/Bootstrap).

## Coding Standards

1. **C# Architecture:** Keep `PageModels` thin. Move heavy business logic (like calling the LLM API) into injected scoped interfaces/services (e.g., `IGenAiService`).
2. **EF Core:** Always use asynchronous EF Core methods (e.g., `ToListAsync()`, `FirstOrDefaultAsync()`).
3. **UI Styling** - When generating "Quizzes/Test Sheets", wrap them in specific print-friendly CSS (e.g., `@media print`) and format them to fit on standard A4 paper.
4. **LLM API Integration:** When writing the Generative AI integration, explicitly instruct the LLM to return strict JSON and include robust `try/catch` JSON deserialization logic in the C# service.

---

## 1. Thin PageModels

PageModels validate input, call services, and return results — nothing more.

```csharp
// ✅ GOOD
public class GenerateQuizModel(IGenAiService genAi) : PageModel
{
  [BindProperty] public QuizRequest Input { get; set; } = new();

  public async Task<IActionResult> OnPostAsync()
  {
    if (!ModelState.IsValid) return Page();
    var quiz = await genAi.GenerateQuizAsync(Input, HttpContext.RequestAborted);
  return RedirectToPage("Preview", new { id = quiz.Id });
  }
}

// ❌ BAD — LLM calls, JSON parsing, or DbContext logic in the PageModel
```

Register services as scoped in `Program.cs`:

```csharp
builder.Services.AddScoped<IGenAiService, GenAiService>();
```

---

## 2. Async EF Core

Never block on database I/O. Use `async`/`await` end-to-end.

```csharp
// ✅ GOOD
var lesson = await _db.Lessons
  .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

var worksheets = await _db.Worksheets
  .Where(w => w.LessonId == lessonId)
  .ToListAsync(cancellationToken);

await _db.SaveChangesAsync(cancellationToken);

// ❌ BAD — ToList(), FirstOrDefault(), SaveChanges() on request paths
```

Pass `CancellationToken` from `HttpContext.RequestAborted` through services when available.

---

## 3. Quiz / Test Sheet Print Layout

Quizzes and test sheets must render on **A4 (210 × 297 mm)** and print cleanly.

- Use Tailwind for printable content; hide nav/buttons with `print:` utilities or `@media print`.
- Do not mix Bootstrap utilities into quiz print views.

```html
<article class="quiz-sheet mx-auto w-[210mm] min-h-[297mm] bg-white p-[15mm]">
  <header class="quiz-header border-b pb-4 mb-6">
    <h1 class="text-xl font-bold">@Model.Title</h1>
    <p class="text-sm text-gray-600">Name: _______________  Date: _______________</p>
  </header>
  <ol class="space-y-6">
    @foreach (var q in Model.Questions) { /* question block */ }
  </ol>
</article>
```

```css
@media print {
  body { margin: 0; background: white; }
  .no-print { display: none !important; }
  .quiz-sheet {
    width: 210mm;
    min-height: 297mm;
    box-shadow: none;
    page-break-after: always;
  }
  .quiz-header { break-after: avoid; }
  .question-block { break-inside: avoid; }
}
```

---

## 4. LLM Integration — Strict JSON

### Prompt the LLM for strict JSON

Include in the system or user prompt:

```
Respond with ONLY valid JSON. No markdown fences, no commentary.
Schema: { "title": string, "questions": [{ "text": string, "options": string[], "correctIndex": number }] }
```

### Deserialize with try/catch in the service

All LLM HTTP calls and JSON parsing belong in `IGenAiService` (or similar), not PageModels.

```csharp
public async Task<QuizDto> GenerateQuizAsync(QuizRequest request, CancellationToken ct)
{
  var raw = await CallLlmAsync(BuildPrompt(request), ct);

  try
  {
    var quiz = JsonSerializer.Deserialize<QuizDto>(raw, JsonOptions)
      ?? throw new JsonException("Deserialized quiz was null.");
    return quiz;
  }
  catch (JsonException ex)
  {
    _logger.LogWarning(ex, "LLM returned invalid JSON. Raw: {Raw}", raw);
    throw new GenAiParseException("The AI response could not be parsed. Please try again.", ex);
  }
}

private static readonly JsonSerializerOptions JsonOptions = new()
{
  PropertyNameCaseInsensitive = true
};
```

On failure, log the raw response, throw a domain exception, and let the PageModel surface a user-friendly message via `ModelState` or `TempData`.

---

## Quick Checklist

- [ ] PageModel delegates to scoped service interfaces
- [ ] All EF queries use `*Async` methods
- [ ] Quiz/test views use A4 sizing + `@media print` rules
- [ ] LLM prompt demands strict JSON; service wraps deserialization in try/catch
