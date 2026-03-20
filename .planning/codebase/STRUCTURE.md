# Codebase Structure

**Analysis Date:** 2026-03-17

## Directory Layout

```
machine_god/
├── src/                    # Source code root
│   ├── pages/             # Next.js pages or route handlers
│   ├── components/        # Reusable React components
│   ├── services/          # Business logic services
│   ├── repositories/      # Data access layer
│   ├── api/               # API route handlers
│   ├── db/                # Database setup, migrations, schemas
│   ├── lib/               # Library/utility modules
│   ├── utils/             # Helper functions and constants
│   ├── hooks/             # Custom React hooks
│   ├── types/             # TypeScript type definitions
│   └── middleware/        # Express/middleware functions
├── public/                # Static assets
├── tests/                 # Test files and fixtures
├── .planning/             # Planning and documentation
│   └── codebase/         # Codebase analysis documents
├── package.json           # Node.js dependencies
├── tsconfig.json          # TypeScript configuration
├── .eslintrc.json         # ESLint configuration
├── jest.config.js         # Jest testing configuration
└── README.md              # Project documentation
```

## Directory Purposes

**src/**
- Purpose: All application source code
- Contains: TypeScript/JavaScript application logic
- Key files: `src/index.ts`, `src/server.ts`, `src/main.tsx`

**src/pages/**
- Purpose: Next.js page routes or SSR page components
- Contains: Page components, route handlers
- Key files: `src/pages/index.tsx`, `src/pages/api/[endpoint].ts`

**src/components/**
- Purpose: Reusable React UI components
- Contains: Functional components, component logic
- Key files: Named by component (e.g., `Button.tsx`, `UserCard.tsx`)

**src/services/**
- Purpose: Business logic layer
- Contains: Service classes/functions implementing domain logic
- Key files: Named by domain (e.g., `userService.ts`, `authService.ts`)

**src/repositories/**
- Purpose: Data access abstraction layer
- Contains: Database query functions and models
- Key files: Named by entity (e.g., `userRepository.ts`, `postRepository.ts`)

**src/api/**
- Purpose: API route handlers and endpoints
- Contains: RESTful or GraphQL endpoint definitions
- Key files: Named by resource (e.g., `users.ts`, `posts.ts`)

**src/db/**
- Purpose: Database configuration and schema
- Contains: Connection setup, migrations, ORM models
- Key files: `schema.ts`, `migrations/`, `client.ts`

**src/lib/**
- Purpose: Reusable library functions and modules
- Contains: External API clients, formatters, parsers
- Key files: Named by functionality (e.g., `stripe.ts`, `aws.ts`)

**src/utils/**
- Purpose: Utility and helper functions
- Contains: Constants, validation, formatting functions
- Key files: `constants.ts`, `validators.ts`, `formatters.ts`

**src/hooks/**
- Purpose: Custom React hooks
- Contains: Reusable state logic for components
- Key files: Named by hook (e.g., `useFetch.ts`, `useAuth.ts`)

**src/types/**
- Purpose: Shared TypeScript type definitions
- Contains: Interfaces and types used across layers
- Key files: `index.ts` (barrel), `user.ts`, `api.ts`

**src/middleware/**
- Purpose: Express.js middleware or Next.js middleware
- Contains: Auth checks, request logging, error handling
- Key files: Named by concern (e.g., `auth.ts`, `logger.ts`)

**public/**
- Purpose: Static assets served directly
- Contains: Images, fonts, client-side manifests
- Key files: `favicon.ico`, `robots.txt`

**tests/**
- Purpose: Test files organized parallel to src
- Contains: Unit tests, integration tests, fixtures
- Key files: `unit/`, `integration/`, `fixtures/`

**.planning/codebase/**
- Purpose: Architecture and quality documentation
- Contains: ARCHITECTURE.md, STRUCTURE.md, CONVENTIONS.md, TESTING.md
- Key files: Analysis documents written by mapping phase

## Key File Locations

**Entry Points:**
- `src/index.ts`: Main application entry point
- `src/server.ts`: Express/server setup
- `src/pages/_app.tsx`: Next.js app wrapper (if using Next.js)
- `src/pages/api/[...route].ts`: API route catch-all

**Configuration:**
- `tsconfig.json`: TypeScript compiler options
- `package.json`: Dependencies and scripts
- `.eslintrc.json`: Linting rules
- `jest.config.js`: Test runner configuration

**Core Logic:**
- `src/services/`: Where business rules live
- `src/repositories/`: Where data queries live
- `src/types/`: Where type contracts are defined

**Testing:**
- `tests/unit/`: Unit test files
- `tests/integration/`: Integration test files
- `tests/fixtures/`: Mock data and test utilities

## Naming Conventions

**Files:**
- kebab-case: `user-service.ts`, `api-client.ts`
- camelCase: `userService.ts`, `apiClient.ts`
- PascalCase for components: `UserCard.tsx`, `Button.tsx`
- Use consistent convention throughout project

**Directories:**
- lowercase: `src/services/`, `src/repositories/`
- plural for collections: `src/components/`, `src/hooks/`
- singular for concerns: `src/middleware/`, `src/lib/`

**Exports:**
- Named exports for services, utilities, types
- Default export for components (optional but common)
- Barrel files in `index.ts` for public APIs

## Where to Add New Code

**New Feature:**
- Primary code: `src/services/[featureName]Service.ts`
- Repository: `src/repositories/[entityName]Repository.ts`
- API routes: `src/api/[resource].ts`
- Tests: `tests/unit/services/[featureName].test.ts`
- Components: `src/components/[Feature]/`

**New Component/Module:**
- Implementation: `src/components/[ComponentName]/[ComponentName].tsx`
- Types: `src/components/[ComponentName]/types.ts`
- Tests: `tests/unit/components/[ComponentName].test.tsx`
- Styles: `src/components/[ComponentName]/styles.ts` (if using CSS-in-JS)

**Utilities:**
- Shared helpers: `src/utils/[category].ts`
- Custom hooks: `src/hooks/use[HookName].ts`
- External API wrappers: `src/lib/[service].ts`
- Types: `src/types/[domain].ts`

## Special Directories

**node_modules/**
- Purpose: Installed npm dependencies
- Generated: Yes
- Committed: No

**.git/**
- Purpose: Git version control
- Generated: Yes
- Committed: No

**.planning/codebase/**
- Purpose: Architecture analysis documents
- Generated: Yes (by mapping phase)
- Committed: Yes

**dist/ or build/**
- Purpose: Compiled output
- Generated: Yes (by build script)
- Committed: No

---

*Structure analysis: 2026-03-17*
