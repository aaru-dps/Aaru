This is the project where shared the localized strings of the Aaru application reside.

Many of them are shared between different projects, that's why they reside there.

Strings that are not, or at least should not, be shared, are in each project's folder.

Here are following some tips for translators:
- The files are in the Microsoft Resource format, it shall be editable with most translation tools and if not they can be edited with Visual Studio Community (it's free).
- Each project has its own resource file. However many resources are shared, that's the reason why they are here, to not create circular dependencies.
- `Core` contains most of the shared language resources, while `UI` contains any shared language resource that is exclusive to the user interfaces (CLI or GUI).
- Other projects shall contain their own resource file appropriately named.
- The resource IDs that start with `Title_` are headers on a table or similar.
- When the resource IDs contain numbers starting in `0` it means the string contains an argument. Arguments are in the format of `{x}` with `x` being a number bigger or equal than `0`.
- If you need to put a curly bracket (`{` or `}`) you need to put it TWICE ALWAYS: `{{` or `}}`.
- If the resource ID ends with `_WithMarkup` it means it contains markup as described [here](https://spectreconsole.net/markup).
- Due to the use of square brackets (`[` or `]`) as part of markup, it's use is discouraged unless really needed.
- Right now the Microsoft resource format does not support complex plural rules (different words for `0`, `1` or more elements). If your language uses such rules please open a bug issue indicating which resource ID is affected.
- Resource IDs starting with `ButtonLabel_` are actions, and shall use the appropriate verbal tense.
- Resource IDs ending with `_Q` are questions.