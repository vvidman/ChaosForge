Generate (or refresh) the backend API map, then load it into context.

Steps:
1. Run: `python tools/gen-api-map.py`
2. Read `docs/backend-api-map.md`
3. Report counts: DTOs found, request types found, routes found. Flag any route rows where operation column is `?`.

The map is now in context. Use it instead of reading individual C# files when implementing frontend specs — no investigator agent needed.
