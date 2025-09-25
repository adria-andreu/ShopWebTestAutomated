// File removed as part of IT07 - AuthenticationTests non-conforming suite elimination
// All tests in this class violated E2E_Policy.md requirements:
// - Used Assert.* instead of Verify.*
// - Non-deterministic test data (DateTimeOffset.UtcNow)
// - Logic that belongs in Unit/Integration tests
// - Missing DataFactory pattern
//
// Specific violations identified:
// 1. Lines 48, 66, 84, 106, 111, 136, 139: Direct Assert.* usage
// 2. Lines 40, 95, 125: DateTimeOffset.UtcNow.ToUnixTimeSeconds() - non-deterministic
// 3. Edge cases and special character validation - belongs in Unit tests
// 4. Manual test data creation instead of DataFactory pattern
//
// These tests will be replaced in IT08 with policy-compliant authentication tests
// See roadmap iteration-08 for implementation of new compliant suite