---
name: BillScanner FE — Project Implementation Guide
description: Concise guide to the BillScanner React Native (Expo) frontend. Covers architecture, conventions, and the patterns to follow when adding features, API integrations, UI components, screens, and tests.
---

# BillScanner FE — Project Guide

## Tech Stack

Expo SDK 54 · TypeScript (strict) · React Query v5 · React Hook Form v7 · Axios + axios-retry · expo-router v6 (file-based routing) · StyleSheet.create + design tokens · Jest + ts-jest · Storybook RN v10

## What the App Does

Scan receipts/bills via camera → upload to Cloudinary (signed) → backend OCR returns structured data with confidence scores → user reviews/edits in a form → save. Also supports VietQR scanning for auto-filling bank payment info.

---

## Architecture

Dependencies flow downward only:

```
app/ (screens)  →  components/form/  →  components/ui/
                         ↓
              features/*/hooks/  (React Query)
                         ↓
              features/*/service  (business logic, singleton classes)
                         ↓
              features/*/api/  (thin Axios wrappers)
                         ↓
              services/  (Axios client, Cloudinary)
                         ↓
              context/ · hooks/ · types/ · utils/ · assets/styles/
```

**Key rules:**
- Features are self-contained — no cross-feature imports.
- Services are singleton classes (`export const xService = new XService()`).
- Hooks wrap services, never raw API calls.
- Components receive data via props — no data fetching in UI components.
- Styles are co-located (`*.styles.ts` using `StyleSheet.create`).

---

## Directory Layout

```
app/                              # Expo Router routes
├── _layout.tsx                   # Root providers (QueryClient, Auth, Loading)
├── auth/                         # Login (unauth)
└── (protected)/                  # Tabs: home, gallery, settings, user
    └── home/scan/                # Camera → preview → process form

features/<name>/                  # Feature module pattern
├── <name>.types.ts               # Domain types/enums
├── api/
│   ├── <name>.api.ts             # Axios call wrappers using apiClient
│   └── <name>.api.types.ts       # Request/Response DTOs
├── hooks/
│   └── use<Action>.ts            # React Query useQuery/useMutation
├── states/                       # State type definitions (if needed)
└── <name>.service.ts             # Business logic class (singleton)

components/
├── ui/<ComponentName>/           # Reusable primitives
│   ├── <Name>.tsx                # Component
│   ├── <Name>.styles.ts          # Styles
│   └── <Name>.stories.tsx        # Storybook
└── form/ProcessForm/             # Bill form sections (VendorDetails, Items, Totals, etc.)

services/api/                     # Shared Axios instance (token injection, retry, error handling)
services/cloudinary/              # Signed upload to Cloudinary
context/                          # AuthContext, LoadingContext
hooks/                            # useAuth(), useLoading()
types/api.types.ts                # ApiResponse<T>, PaginatedResponse<T>
assets/styles/theme.ts            # Colors, Spacing, FontSize, BorderRadius, etc.
tests/features/                   # Integration tests (hit real backend)
```

---

## Adding a New Feature

Follow the existing feature modules (`features/auth/`, `features/images/`, `features/file-storage/`):

1. **Types** → `features/<name>/api/<name>.api.types.ts` for DTOs, `<name>.types.ts` for domain types
2. **API** → `features/<name>/api/<name>.api.ts` — thin wrappers using `apiClient` from `services/api/api.client`
3. **Service** → `features/<name>/<name>.service.ts` — singleton class calling API, unwraps `response.data`
4. **Hooks** → `features/<name>/hooks/use<Action>.ts` — React Query hooks calling service methods. Follow the `Omit<..., "mutationFn">` options pattern and invalidate relevant queries in `onSuccess`.
5. **Screen** → `app/` route file consuming the hooks

Reference `features/images/` for a clean query/mutation example, `features/auth/` for a complex service with state management.

## Adding a UI Component

Three files in `components/ui/<Name>/`: component, styles, storybook. Use design tokens from `assets/styles/theme.ts`. Support style prop composition. See existing components (Button, Card, Input) for the pattern.

## Adding a Form Section

Follow existing sections in `components/form/ProcessForm/` (VendorDetails, Totals, Items). Pattern: `<Card>` wrapper → RHF `<Controller>` → `<ConfidenceWrapper>` (for OCR fields) → input. The `ReceiptForm` type lives in `app/(protected)/home/scan/process.tsx`.

## Adding a Screen

Expo Router file-based routing. Co-locate `*.styles.ts`. Use `SafeAreaView` as top-level container. Navigate with `useRouter()`, read params with `useLocalSearchParams()`. Auth guards are handled by `RootNavigator` via `Stack.Protected`.

---

## Important Conventions

| What | Convention |
|------|-----------|
| API calls | Always use `apiClient` from `services/api/api.client` (never raw `fetch` except Cloudinary uploads) |
| Module resolution | Root-relative imports via `baseUrl: "."` — e.g. `import { apiClient } from "services/api/api.client"` |
| Auth token | Injected automatically by `apiClient` interceptor. Use `setTokenProvider()` to avoid circular deps |
| 401 handling | Automatic via React Query caches → `ApiErrorHandler` → logout + redirect |
| Loading overlay | `useLoading()` → `startLoading(msg)` / `stopLoading()` for blocking operations |
| Env vars | `EXPO_PUBLIC_*` prefix (Expo convention). Defined in `.env.development` |
| Naming | `PascalCase` for components/folders, `kebab-case` for feature files, `use<Action><Entity>` for hooks |
| Styles | Co-located `*.styles.ts`, always use theme tokens — never hardcode colors/spacing |
| Tests | Integration tests in `tests/features/`, test against real backend via service layer |

---

## Gotchas

- **Circular dep prevention**: `authService` uses `setTokenProvider()` instead of importing `apiClient` directly.
- **`ReceiptForm` type** is defined in `app/(protected)/home/scan/process.tsx`, not a types file. Update it AND the `toDefaultValues()` mapper when adding fields.
- **ESLint config** references boilerplate plugins (vitest, tailwind, testing-library) that aren't fully set up — rely on TypeScript strict mode for safety.

