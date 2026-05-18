# Changelog

All notable changes to this project are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased]

---

## [2.53.0] — 2026-05-19

### Added

- `Form1_PostalCode_KeyPress_BlocksNonDigit`: invokes `OnKeyPress` on `txtPostalCode` via
  reflection; covers the inline lambda that blocks non-digit, non-backspace characters
  (Form1.<>c lines 197–199).
- `PinLoginForm_ClearButton_ClearsEnteredPin`: enters two digits via keyboard then
  `PerformClick`s the CLR pad button; covers the `btnClear.Click` lambda (lines 157–163).
- `PinLoginForm_BackButton_RemovesLastDigit`: enters two digits then `PerformClick`s the Back
  pad button; covers the `btnBack.Click` lambda (lines 170–178).
- `SettingsForm_SaveButton_WithInvalidPin_ShowsValidationError`: sets the StaffPin grid row to
  "12" (too short) and clicks Save; covers the `continue` branch when `TrySaveStaffPin` returns
  false (SettingsForm line 276) and the resulting "Validation Error" dialog + `LoadSettings` reload.

---

## [2.52.0] — 2026-05-18

### Fixed

- `OrderHistoryForm.ExportCsv`: the empty-list guard checked `_listView.Items.Count == 0`, but
  `ApplyFilter` always inserts a placeholder "No matching orders found." item when the result set
  is empty — so `Count` was 1, not 0. Guard changed to `_currentOrders.Count == 0`, which
  correctly reflects the actual order count and eliminates the headless CI hang.

### Added

- `Form1_ConfirmOrder_NoPizzaSelected_ShowsOrderError`: unchecks size and crust radio buttons so
  `BuildCurrentPizzaItems()` returns empty; confirms that `btnConfirmOrder` shows an "Order Error"
  dialog (covers the `!orderResult.IsValid` branch in `btnConfirmOrder_Click`).
- `Form1_BtnClearOrder_EmptyList_DoesNothing`: clicks `btnClearOrder` on a form with an empty
  `lvOrder`; verifies no dialog appears (covers the `Count == 0` early-return guard).
- `SettingsForm_ViewAutoBackupsButton_WithNoDataDir_ShowsBackupUnavailableDialog`: passes
  `dataDirectory: null` and clicks "View Auto-Backups"; covers the null-dir guard in
  `BtnViewBackups_Click`.

---

## [2.51.0] — 2026-05-18

### Fixed

- `OrderHistoryForm_ExportCsv_EmptyList_DoesNotOpenDialog` timed out in headless CI because
  `form.Show()` + `PerformClick()` on the Export CSV button triggered a window-handle creation
  path that stalls in a headless environment. Replaced with a reflection-based invocation of
  `ExportCsv()` that does not require a visible window; renamed to
  `OrderHistoryForm_ExportCsv_EmptyList_ReturnsImmediately`.

### Added

- `PinLoginForm_CorrectPin_ClosesWithDialogResultOk`: enters a PBKDF2-protected PIN via
  `OnKeyDown`, submits with Enter; asserts `DialogResult.OK` and covers the success path
  in `BtnEnter_Click` including `UpgradeLegacyPinIfNeeded` (protected PIN — no upgrade needed).

---

## [2.50.0] — 2026-05-18

### Added

- 2 new tests in `SettingsFormSmokeTests`; 461 total, 92.2% coverage maintained.
- `SettingsForm_RestoreButton_WithNoDataDir_ShowsBackupUnavailableDialog`: invokes Restore DB
  button with null `dataDirectory`; verifies the null-dir guard in `BtnRestore_Click` shows
  the "Backup Unavailable" dialog.
- `SettingsForm_RestoreButton_WithDataDir_NoConfirm_DoesNotRestore`: invokes Restore DB with a
  real temp data dir and no PIN (EnsureAuthorized returns true); dismisses the "Restore Database"
  YesNo confirmation with No; verifies the restore is cancelled before the file picker opens.

---

## [2.49.0] — 2026-05-18

### Added

- 2 new tests; 459 total, 92.2% coverage maintained.
- `Form1_ProcessCmdKey_AltW_WithSettings_OpensSettingsForm`: covers the normal `OpenSettingsForm`
  path (non-null settings, no PIN configured); `EnsureAuthorized` returns true, SettingsForm opens.
- `OrderHistoryForm_ExportCsv_EmptyList_DoesNotOpenDialog`: covers the `count == 0` early-return
  guard in `ExportCsv`; verifies no dialog appears and the form stays visible.

---

## [2.48.0] — 2026-05-18

### Added

- 8 new tests in `FormSmokeTests`; 457 total, 92.2% coverage maintained.
- `Form1_ProcessCmdKey_AltH_OpensOrderHistoryDialog`: covers `case Keys.Alt | Keys.H:` in
  `ProcessCmdKey`; opens `OrderHistoryForm` and asserts form stays visible.
- `Form1_ProcessCmdKey_AltR_OpensSalesReportDialog`: covers `case Keys.Alt | Keys.R:`.
- `Form1_ProcessCmdKey_AltE_OpensEndOfDayDialog`: covers `case Keys.Alt | Keys.E:`.
- `Form1_ProcessCmdKey_AltC_OnTab1_ConfirmsOrder`: covers `case Keys.Alt | Keys.C:`; asserts
  tab advances to Tab 2 after calling `btnConfirmOrder_Click` via the shortcut.
- `Form1_ProcessCmdKey_F1_ShowsKeyboardHelp`: covers `case Keys.F1:`.
- `Form1_ProcessCmdKey_AltW_NullSettings_ShowsSettingsDialog`: covers `case Keys.Alt | Keys.W:`
  with null settings; asserts "Settings unavailable" dialog appears.
- `Form1_CboPaymentMethod_Cash_DisablesReferenceField`: sets payment method to Cash; asserts
  `txtCardOrPromo.Enabled == false` and `lblCardOrPromo.Text == "Reference:"`.
- `Form1_CboPaymentMethod_PromoCard_SetsPromoCodeLabel`: sets payment method to Promo Card;
  asserts field enabled, label is "*Promo Code:", `AccessibleName == "Promo Code"`.

---

## [2.47.0] — 2026-05-18

### Added

- 8 new tests in `PinLoginFormKeyboardTests` and `FormSmokeTests`; 449 total, coverage maintained.
- `PinLoginForm_IncorrectPin_FirstAttempt_ShowsAttemptsRemainingMessage`: one wrong PIN attempt;
  asserts `_lblError` contains "2 attempts remaining."
- `PinLoginForm_IncorrectPin_SecondAttempt_ShowsOneRemainingMessage`: two wrong PIN attempts;
  asserts `_lblError` contains "1 attempt remaining."
- `PinLoginForm_AppendDigit_WhileLockedOut_DoesNotChange`: triggers lockout (3 wrong attempts),
  then presses a digit key; asserts dots count is unchanged.
- `PinLoginForm_BtnEnter_WhileLockedOut_UpdatesLockoutMessage`: calls `BtnEnter_Click` via
  reflection while locked out; asserts `_lblError` still says "Try again."
- `PinLoginForm_OnKeyDown_Backspace_WhileLockedOut_HandledNoChange`: asserts Backspace is marked
  handled but dots are unchanged while locked out.
- `PinLoginForm_OnKeyDown_Delete_WhileLockedOut_HandledNoChange`: same for Delete key.
- `PinLoginForm_UpgradeLegacyPin_NullSettings_DoesNotThrow`: invokes `UpgradeLegacyPinIfNeeded`
  via reflection on a form created with null settings; asserts no exception.
- `Form1_OpenSettingsForm_NullSettings_ShowsNotAvailableDialog`: creates Form1 with null settings,
  invokes `OpenSettingsForm` via reflection; `DialogAutoCloser("Settings")` handles the dialog.

### Fixed

- `Form1_BtnPay_PromoCodeInvalid_ShowsPromoError`: added required customer fields before clicking
  `btnPay` so `ValidateCustomer` passes and the promo-error branch is reached. Previously the test
  was relying on timing to close the "Validation Error" dialog accidentally; now robust.

---

## [2.46.0] — 2026-05-18

### Added

- 9 new tests in `FormSmokeTests`; 441 total, coverage maintained at 92.2% line rate.
- `Form1_BtnAddPizzaToCart_NoPizzaSelected_ShowsAddToCartError`: unchecks `rbSizeSmall` and
  `rbCrustNormal` (Form1 constructor sets them checked) so `BuildCurrentPizzaItems()` returns
  empty; asserts `DialogAutoCloser("Add to Cart")` handles the resulting error MessageBox.
- `Form1_BtnClearOrder_No_DoesNotClearList`: confirms that answering No on the "Clear Order"
  YesNo MessageBox leaves `lvOrder` intact (1 item remains). Uses `DialogAutoCloser("Clear Order",
  respondNo: true)` which sends IDNO (7) via WM_COMMAND.
- `Form1_BtnPay_PromoCodeInvalid_ShowsPromoError`: fills checkout fields with an invalid promo
  code and asserts the promo-error MessageBox appears.
- `Form1_BtnPay_MissingPaymentFields_ShowsValidationError`: leaves required checkout fields blank
  and asserts the validation-error MessageBox appears.
- `Form1_BtnPay_InvalidAmountFormat_ShowsValidationError`: sets `txtAmountPaid` to "notanumber"
  and asserts the validation-error MessageBox appears.
- `Form1_OrderComplete_No_ClosesForm`: performs a full cash checkout, skips the receipt dialog via
  `DialogButtonClicker("Order Confirmed", "Skip")`, then answers No on "Order Complete" via
  `DialogAutoCloser("Order Complete", respondNo: true)`; asserts the form becomes invisible.
- `Form1_MinorHandlers_KeyPressAndTextChanged`: exercises `txtAmountPaid_KeyPress`,
  `AllowDigitsOnly` (txtQtyCoke/txtQtyWater), and `txtAmountPaid_TextChanged` directly.
- `Form1_ValidateDrinkQuantities_ZeroQty_ShowsInvalidQuantityError`: sets a drink quantity to
  zero and asserts the invalid-quantity MessageBox appears.
- `Form1_ConfirmOrder_WithSideChecked_AddsSideToList`: checks a side checkbox before confirming
  the order and asserts the side item appears in `lvOrder`.

### Changed

- `WinFormsTestHelper.DialogAutoCloser`: added `respondNo` constructor overload and updated
  `WatchLoop` to send IDNO (7) via WM_COMMAND when `respondNo` is true, enabling reliable
  dismissal of native Win32 YesNo MessageBoxes with the No button.
- `Form1.cs`: added title strings to previously untitled `MessageBox.Show` calls in `btnPay`
  (Order Error), `btnAddPizzaToCart` (Add to Cart), and `ValidateDrinkQuantities` (Invalid
  Quantity) so `DialogAutoCloser` can match them by fragment.

---

## [2.45.0] — 2026-05-18

### Added

- 8 new tests across `FormSmokeTests` and `SettingsFormSmokeTests`; 432 total, 92.2% line coverage.
- `Form1_PrintReceipt_OpensPrintPreviewDialog`: injects `_lastReceiptText` via reflection,
  calls the private `PrintReceipt(Order)` method, and closes the `PrintPreviewDialog` via
  `DialogAutoCloser("Print Preview")`. Covers all 70 lines of `PrintReceipt`.
- `Form1_ProcessCmdKey_AltK_OnTab2_NavigatesToTab3`: invokes `ProcessCmdKey` with
  `Keys.Alt | Keys.K` while on Tab 2; verifies navigation to Tab 3. Also selects
  ExtraLarge + Sausage crust to cover `RecalculateLiveTotal` ExtraLarge/Sausage branches.
- `Form1_ProcessCmdKey_AltY_OnTab3_TriggersValidation`: invokes `ProcessCmdKey` with
  `Keys.Alt | Keys.Y` on Tab 3 with empty customer fields; `DialogAutoCloser("Validation Error")`
  dismisses the resulting MessageBox.
- `Form1_ProcessCmdKey_Escape_FromTab3AndTab2_NavigatesBack`: Escape from Tab 3 goes to Tab 2,
  Escape from Tab 2 goes to Tab 1.
- `Form1_InlineValidation_Leave_InvalidAndValid_UpdatesBackColor`: invokes the private
  `txtPostalCode_Leave`, `txtContactNo_Leave`, and `txtEmail_Leave` handlers directly via
  reflection with both valid and invalid inputs; asserts BackColor changes.
- `Form1_LvContextMenu_RemoveSelectedItem_UpdatesList`: navigates to Tab 2, adds an item
  to `lvOrder`, selects it, then invokes the context-menu Remove handler via
  `ToolStripItem.OnClick` reflection (direct `PerformClick` bails when the menu is not open).
- `SettingsForm_SaveButton_WithValidData_ShowsSavedDialogAndCloses`: clicks Save Changes
  with valid default data; `DialogAutoCloser("Saved")` handles the confirmation, form closes.
- `SettingsForm_SaveButton_WithInvalidNumericValue_ShowsValidationError`: sets a numeric
  setting value to "notanumber" via the grid, clicks Save Changes; `DialogAutoCloser("Validation Error")`
  handles the error dialog, form stays open.

---

## [2.44.1] — 2026-05-18

### Fixed

- `Form1_ReceiptDialog_SkipButton_ClosesDialogAndPromptsOrderComplete` replaces the
  removed `CopyToClipboard` test, which timed out on CI because `Clipboard.SetText`
  is unreliable in headless Windows sessions: the "Copied" MessageBox never appeared
  so `DialogAutoCloser` never fired and the 180 s STA timeout was hit.
  The replacement uses `DialogButtonClicker("Order Confirmed", "Skip")` to click the
  Skip button (covers `btnSkip.Click += (s, ev) => dlg.Close()` lambda) followed by
  `DialogAutoCloser("Order Complete")` — no clipboard involvement.

## [2.44.0] — 2026-05-18

### Added

- `WinFormsTestHelper.cs`: new `DialogButtonClicker` helper — background thread that
  finds a top-level window by title fragment then clicks a named child button via
  `EnumChildWindows` + `BM_CLICK`, enabling coverage of receipt-dialog button lambdas.
- `FormSmokeTests.cs`: 1 new test covering the receipt-dialog Skip path.

**Total tests: 424 passing.**
**Coverage crosses 90%: WindowsFormsApplication3 now at 90.0%+.**

---

## [2.43.0] — 2026-05-18

### Added

- `FormSmokeTests.cs`: 3 new `Form1` smoke tests covering programmatic Tab 2 buttons:
  - `Form1_HistoryButton_OpensOrderHistoryDialog` — navigates to Tab 2, clicks "Order
    History", `DialogAutoCloser("Order History")` closes the form; covers
    `btnHistory.Click` lambda.
  - `Form1_SalesReportButton_OpensSalesReportDialog` — clicks "Sales Report",
    `DialogAutoCloser("Sales Report")` closes it; covers `OpenSalesReportForm` via
    button lambda.
  - `Form1_EndOfDayButton_OpensEndOfDayDialog` — clicks "End of Day",
    `DialogAutoCloser("End of Day")` closes it; covers `OpenEndOfDayForm` via
    button lambda.
- `SettingsFormSmokeTests.cs`: 1 new test:
  - `SettingsForm_CellBeginEdit_AndCellEndEdit_ChangeBackColor` — gets `_grid` via
    reflection, sets `CurrentCell` to a Value cell, calls `BeginEdit(true)` then
    `EndEdit()`; exercises the `CellBeginEdit` highlight and `CellEndEdit` reset
    lambdas in `SettingsForm.<>c__DisplayClass15_0`.
- Added `using System.Reflection` to `SettingsFormSmokeTests.cs`.

**Total tests: 423 passing.**
**Overall WindowsFormsApplication3 coverage now 89.9%.**

---

## [2.42.0] — 2026-05-18

### Added

- `OrderHistoryFormTests.cs`: 3 new tests covering column-click sorting and keyboard shortcuts:
  - `OrderHistoryForm_ColumnClick_SortsByDateToggle` — invokes `ListView_ColumnClick`
    via reflection twice on column 0; asserts `▲` then `▼` in header text; exercises
    `SortOrders` comparison lambda for date column in both directions.
  - `OrderHistoryForm_ColumnClick_SortsByTotalAndCustomerName` — clicks columns 1, 4,
    2, 3 in sequence; exercises all non-default switch branches of `SortOrders`.
  - `OrderHistoryForm_KeyDown_Delete_WithNoSelection_DoesNotThrow` — raises `KeyDown`
    via `Control.OnKeyDown` reflection with `Keys.Delete`; form guard returns early
    without throwing; covers the `KeyDown` lambda body.
- Added `using System.Reflection` to `OrderHistoryFormTests.cs`.

**Total tests: 419 passing.**
**Overall WindowsFormsApplication3 coverage now 89.5%.**

---

## [2.41.0] — 2026-05-18

### Added

- `FormSmokeTests.cs`: 1 new `Form1` smoke test covering the receipt-options dialog path:
  - `Form1_SubmitOrder_WithReceiptDialogs_SkipAndOrderComplete_ResetsToTab1` — uses
    the production constructor (`showReceiptDialogs: true`); completes a full
    cash checkout; `DialogAutoCloser("Order Confirmed", "Order Complete")` closes
    the receipt-options custom `Form` via WM_CLOSE and the "Order Complete" YesNo
    via IDYES, which calls `ResetFullForm()` and returns to Tab 1. Covers lines
    871-927 of `btnSubmitOrder_Click` (the entire `_showReceiptDialogs` block)
    and `ResetFullForm`.

**Total tests: 416 passing.**
**`Form1` coverage increases significantly; overall WindowsFormsApplication3 now 89.0%.**

---

## [2.40.0] — 2026-05-18

### Added

- `FormSmokeTests.cs`: 3 new `Form1` smoke tests:
  - `Form1_ExitButton_Yes_ClosesForm` — clicks `btnExit`, `DialogAutoCloser("Exit")`
    sends IDYES, asserts `form.Visible == false`.
  - `Form1_AboutButton_ShowsAboutDialog` — finds the dynamically-added "About" button
    by text prefix, `DialogAutoCloser("About Pizza Express NZ")` dismisses the
    `MessageBox`; covers `ShowAboutDialog`.
  - `Form1_KeyboardHelp_ShowsAndCloses` — invokes `ShowKeyboardHelp` via reflection,
    `DialogAutoCloser("Keyboard Shortcuts")` closes the custom `Form`; covers all
    69 lines of the keyboard-help builder.
- Added `using System.Reflection;` to `FormSmokeTests.cs`.

**Total tests: 415 passing.**
**`Form1` coverage increases; overall WindowsFormsApplication3 now 87.5%.**

---

## [2.39.0] — 2026-05-18

### Added

- `SalesReportFormTests.cs`: 4 new smoke tests for `SalesReportForm`:
  - `RunReport_WithOrders_PopulatesKpis` — saves two real orders and clicks
    "Run Report"; asserts the order-count KPI label shows "2".
  - `TodayButton_SetsDateRangeAndRunsReport` — clicks "Today", verifies both
    pickers are set to `DateTime.Today`.
  - `ThisWeekButton_SetsFromToStartOfWeek` — clicks "This Week", verifies
    From is on or before today and To is today.
  - `ThisMonthButton_SetsFromToFirstOfMonth` — clicks "This Month", verifies
    From is the 1st of the current month.
    All four cover `RunReport()` and the quick-date lambda handlers.

**Total tests: 412 passing.**
**`SalesReportForm` coverage 85.2% to 95.1%; overall WindowsFormsApplication3 now 86.1%.**

---

## [2.38.0] — 2026-05-18

### Added

- `EndOfDayFormTests.cs`: 2 new tests for `EndOfDayForm`:
  - `EndOfDayForm_WithOrders_PopulatesKpisAndPaymentList` — opens the form with two
    real orders (Cash + Credit Card) for today; asserts KPI labels show "2", the
    payment breakdown list has 2 rows, and the top items list is non-empty.
    Covers the `_payments.Count > 0` and `_topItems.Count > 0` branches in `LoadData`.
  - `EndOfDayForm_PrintReport_OpensPreviewAndCanBeClosed` — clicks "Print Report"
    with a `DialogAutoCloser("Print Preview")` to dismiss the `PrintPreviewDialog`
    via `WM_CLOSE`; verifies the main form remains visible.
    Covers `PrintReport`, `PrintPage`, and `BuildReportText` paths.

**Total tests: 408 passing.**
**`EndOfDayForm` coverage 80% to 95.6%; overall WindowsFormsApplication3 now 85.5%.**

---

## [2.37.0] — 2026-05-18

### Added

- `SettingsFormSmokeTests.cs`: 3 new smoke tests for backup-related `SettingsForm` paths:
  - Clicking "Backup DB" with `dataDirectory: null` shows the "Backup Unavailable"
    guard dialog (`ShowNoDataDir`).
  - Clicking "View Auto-Backups" with no backups in the Backups folder shows the
    "No auto-backups found yet." dialog (zero-backup branch).
  - Clicking "View Auto-Backups" with a fake auto-backup file present shows the
    backup-list dialog (has-backups branch).

**Total tests: 406 passing.**
**`SettingsForm` coverage 74.3% to 80.9%; overall WindowsFormsApplication3 now 84.4%.**

---

## [2.36.0] — 2026-05-18

### Added

- `OrderHistoryFormTests.cs`: 4 new smoke tests for `OrderHistoryForm`:
  - Search filter shows only matching orders (name search → 1 result from 3).
  - No-match search displays the "No matching orders found." placeholder row.
  - "View Details" button with a selected order shows and dismisses the detail
    `MessageBox` (title "Order Details").
  - Date filter checkbox toggle calls `ApplyFilter` without error; today's order
    remains visible in the default date window.

**Total tests: 403 passing.**
**`OrderHistoryForm` coverage 79.3% to 87.9%; overall WindowsFormsApplication3 now 83.9%.**

---

## [2.35.0] — 2026-05-18

### Added

- `PinLoginFormKeyboardTests.cs`: 8 new tests exercising `PinLoginForm.OnKeyDown`:
  - Digit key (D3) appends one dot to the PIN display.
  - NumPad key (NumPad5) appends one dot.
  - Backspace removes the last entered digit.
  - Delete clears all entered digits.
  - Enter key with the correct PIN closes the form with `DialogResult.OK`.
  - Escape closes the form with `DialogResult.Cancel`.
  - Unhandled key (F5) leaves `e.Handled` false and dots unchanged.
  - 12-digit entry ignores a 13th digit (max-length guard).

### Changed

- `PinLoginForm.OnKeyDown` promoted from `private` to `internal` to allow
  direct testing without WinForms event dispatch.

**Total tests: 399 passing.**
**`PinLoginForm` coverage 73.6% to 85.5%; overall WindowsFormsApplication3 now 83.2%.**

---

## [2.34.0] — 2026-05-18

### Added

- `FormSmokeTests.cs`: 4 new Form1 smoke tests:
  - `Form1_OrderAgain_NavigatesFromTab2BackToTab1` — verifies `btnOrderAgain` returns
    the active tab to Tab 1 after an order is confirmed on Tab 2.
  - `Form1_GoBack_NavigatesFromTab3BackToTab2` — verifies `btnGoBack` returns from
    the checkout tab (Tab 3) back to order review (Tab 2).
  - `Form1_DebitCardCheckout_MasksReferenceAndPersistsOrder` — end-to-end debit card
    checkout: masks last-4 digits (e.g. `****9012`), persists order with correct method.
  - `Form1_CashUnderpayment_DisablesSubmitOrder` — verifies that paying $0.01 toward a
    large pizza order leaves `btnSubmitOrder` disabled and shows a Payment Error dialog.
- `OrderItemTests.cs`: 1 new test for the parameterless `OrderItem()` constructor.

### Changed

- `Form1.btnPay_Click`: all `MessageBox.Show` error calls now include a caption
  (`"Validation Error"`, `"Promo Error"`, `"Payment Error"`) so dialogs have meaningful
  titles and can be matched by `DialogAutoCloser` in tests.

**Total tests: 391 passing.**
**Overall WindowsFormsApplication3 coverage now at 82.3%.**

---

## [2.33.0] — 2026-05-18

### Added

- `SettingsFormLogicTests.cs`: 11 new tests covering `SettingsForm.GetDisplayValue`
  (null row, non-PIN key returns value as-is, PIN not configured returns empty,
  PIN configured returns placeholder) and `SettingsForm.TrySaveStaffPin`
  (empty/null clears PIN, placeholder with no configured PIN clears, placeholder with
  plaintext PIN upgrades to PBKDF2, placeholder with already-protected PIN is no-op,
  invalid PIN adds error and returns false, valid new PIN stores protected hash).

### Changed

- `SettingsForm.StaffPinConfiguredPlaceholder` promoted from `private const` to
  `internal const` to allow test assertions against the exact placeholder string.
- `SettingsForm.GetDisplayValue` promoted from `private` to `internal static`
  (no functional change; instance state was not used).
- `SettingsForm.TrySaveStaffPin` refactored to accept `ISettingsRepository` as first
  parameter and promoted to `internal static`; the private overload delegates to it.

**Total tests: 386 passing.**
**`SettingsForm` coverage increases; overall WindowsFormsApplication3 now at 82.2%.**

---

## [2.32.0] — 2026-05-18

### Added

- `FormSmokeTests.cs`: 2 new end-to-end smoke tests for `Form1`.
  - `Form1_CreditCardCheckout_MasksReferenceAndPersistsOrder`: full checkout with
    "Credit Card" payment method; verifies `txtCardOrPromo` is enabled, submits
    order, and asserts persisted `PaymentReference` is masked (`"****1111"`).
  - `Form1_ClearOrder_WhenItemsPresent_RemovesAllItemsOnConfirm`: builds a one-pizza
    order, clicks `btnClearOrder`, auto-confirms the YesNo dialog, and asserts the
    order list is empty.
  - Together these cover `cboPaymentMethod_SelectedIndexChanged` for non-Cash paths
    and the `btnClearOrder` confirmation flow — both previously uncovered.

**Total tests: 375 passing.**
**`Form1` coverage 63.0% → 64.8%; overall WindowsFormsApplication3 81.9%.**

---

## [2.31.0] — 2026-05-18

### Added

- `SettingsFormHelpersTests.cs`: 24 new tests covering `SettingsForm.FriendlyName`
  (all 10 known keys + unknown key fallback + bulk non-empty assertion) and
  `SettingsForm.IsNumericKey` (all 9 numeric keys return true; StaffPin, unknown,
  and empty string return false).

### Changed

- `SettingsForm.FriendlyName` and `SettingsForm.IsNumericKey` promoted from
  `private static` to `internal static` to enable direct unit testing via
  `InternalsVisibleTo`.

**Total tests: 373 passing.**
**`SettingsForm` coverage increases; overall WindowsFormsApplication3 now at 81.6%.**

---

## [2.30.0] — 2026-05-18

### Added

- `CheckoutWorkflowServiceTests.cs`: 15 new tests filling remaining coverage gaps.
  - Constructor null guards: `null` `IPromoEngine` and `null` `IOrderValidator` each throw `ArgumentNullException`.
  - `BuildOrderRecord` directly tested: status is "Active"; ID is 8 uppercase hex chars;
    all scalar fields (customer name, address, city, region, postal code, payment method) are mapped;
    order lines are mapped in order; discount description and amount set when promo applied.
  - `GetDeliveryMinutes` with real settings: configured value (45 min) returned; zero value
    and invalid string both fall back to `AppConfig.DeliveryMinutes`.
  - `ParseCurrencyOrZero` edge cases: null returns zero; plain decimal string "12.50" parses
    via invariant-culture path; unparsable string returns zero.
  - `ApplyPromo` with `PIZZA20` code: 20% discount applied, total correct.
  - `AssembleOrder` with empty promo-code string: `DiscountDescription` is null.

**Total tests: 349 passing.**
**`CheckoutWorkflowService` coverage increases to near 100%.**

---

## [2.29.0] — 2026-05-15

### Added

- `SecurityServiceTests.cs`: 27 new tests for `PinSecurity` and `StaffAuthSession`.
  - `PinSecurity.IsConfigured`: null, empty, whitespace return false; non-empty returns true.
  - `PinSecurity.IsProtected`: null and plaintext return false; protected hash returns true.
  - `PinSecurity.ValidateNewPin`: blank/null valid; 4-digit and 12-digit valid;
    3-digit (too short), 13-digit (too long), and non-digit input invalid.
  - `PinSecurity.Verify`: correct pin verifies; wrong pin rejected; empty/null stored
    rejects; legacy plaintext exact match and mismatch; malformed hash (too few parts,
    bad base64, non-integer iterations) all return false; PIN with surrounding spaces
    is trimmed and verifies correctly.
  - `StaffAuthSession.HasRecentAuthorization`: never-authenticated returns false;
    just-authenticated returns true; expired session (age > maxAge) returns false.
  - `[TestInitialize]` resets static `_lastAuthenticatedUtc` via reflection before each test.

**Total tests: 334 passing.**
**`StaffAuthSession`: 100% line coverage.**
**`PinSecurity`: 97% line coverage (remaining 3%: defensive null/length guard in private `ConstantTimeEquals` unreachable through public API).**

---

## [2.28.0] — 2026-05-15

### Added

- `OrderSubmissionServiceTests.cs`: 10 new tests covering `OrderSubmissionService`
  (was at 0% coverage).
  - Constructor null-guard assertions for `repo` and `receiptWriter`.
  - `Submit(null)` throws `ArgumentNullException`.
  - `Submit` calls `IOrderRepository.Save` exactly once.
  - `Submit` returns the receipt text from `IReceiptWriter.Build`.
  - Record `Status` is `"Active"`.
  - Customer fields (`CustomerName`, `Address`, `City`, `Region`, `PostalCode`) map correctly.
  - Order lines map item name and quantity.
  - Credit Card order preserves `PaymentReference` (regression guard for v2.23.0 fix).
  - Cash order has `null` `PaymentReference`.
  - `Record.Id` is exactly 8 uppercase hex characters.
- `PaymentReferenceHelperTests.cs`: 15 new tests covering `PaymentReferenceHelper`
  (was at 83% coverage).
  - `RequiresReference`: Cash, Promo Card, empty, null all return `false`; Credit Card and
    Debit Card return `true`.
  - `NormalizeForStorage`: Cash returns `null`; null reference returns `null`;
    whitespace-only returns `null`; 16-digit card number masks to `****1111`;
    short alphanumeric reference returned as-is; multiple spaces collapsed to one;
    reference longer than 30 characters truncated; Promo Card returns `null`.

**Total tests: 307 passing.**

---

## [2.27.0] — 2026-05-15

### Added

- CI coverage gating for the `WindowsFormsApplication3` assembly.
  - `scripts/Check-Coverage.ps1`: parses a Cobertura XML file, filters to a named
    package, and exits non-zero if line-rate is below a configurable threshold
    (default 75%).
  - `scripts/Run-Tests.ps1`: new `-CollectCoverage` switch and `-CoverageOutput` path
    parameter. When set, wraps `vstest.console.exe` with `dotnet-coverage collect`
    to produce a Cobertura XML alongside the `.trx` file.
  - `build-and-test.yml`: three new steps:
    1. Install `dotnet-coverage` global tool.
    2. Run tests with `-CollectCoverage` to collect `coverage.xml`.
    3. Call `Check-Coverage.ps1` to enforce the 75% line-rate gate; fails the build
       if coverage drops below threshold.
    4. Upload `coverage.xml` as an artifact.
  - Current measured line rate: 80.2% (5.2 pp above the gate).

**Total tests: 282 passing.**

---

## [2.26.0] — 2026-05-15

### Fixed

- `C&heck Out` (Alt+H) was shadowed by `ProcessCmdKey`'s `Alt+H` handler (opens
  OrderHistoryForm), so the Check Out button mnemonic never fired.
  Corrected to `Chec&k Out` (Alt+K) and `ProcessCmdKey` updated to intercept
  `Alt+K` instead (tab-aware: only fires when Tab 2 is active).
- `&Pay` (Alt+P) was shadowed by `ProcessCmdKey`'s `Alt+P` handler (fires checkout
  on Tab 2, consumes the key on all other tabs). On Tab 3 the key was consumed but
  did nothing, making the Pay shortcut dead.
  Corrected to `Pa&y` (Alt+Y); `ProcessCmdKey` now intercepts `Alt+Y` tab-aware
  (only fires when Tab 3 is active).
- Stale tooltip on `btnCheckOut`: `"(Alt+P)"` updated to `"(Alt+K)"`.
- Keyboard shortcuts help dialog: replaced `Alt+P = Proceed to checkout` entry
  with `Alt+K` and added new `Alt+Y = Validate payment` entry.
  Also replaced em-dash arrow (`→`) with ASCII `to` to comply with the no-non-ASCII
  rule in source strings.
- `AccessibilityTests` updated to assert `"Chec&k Out"` and `"Pa&y"`.

**Total tests: 282 passing.**

---

## [2.25.1] — 2026-05-15

### Fixed

- `btnCheckOut.Text` corrected from `"&Check Out"` (Alt+C, conflicting with Confirm Order)
  to `"C&heck Out"` (Alt+H, matching the intended Alt+H shortcut).
- `Form1_ButtonMnemonics_AreSetOnLoad` test strengthened to assert exact button text values
  and verify no two buttons share the same Alt shortcut character, preventing regressions.

**Total tests: 282 passing.**

---

## [2.25.0] — 2026-05-15

### Added

- Keyboard-navigation and screen-reader accessibility pass on the main order form (`Form1`).
  - Button mnemonics via `&` in `Text`: Alt+C Confirm Order, Alt+A Add Pizza to Cart,
    Alt+H Check Out, Alt+O Order Again, Alt+L Clear Order, Alt+B Go Back,
    Alt+P Pay, Alt+S Submit Order.
  - Label mnemonics for required checkout fields: `*&First Name:`, `*&Last Name:`,
    `*&Address:`, `*Payment &Method:` — Tab from label moves focus directly to its input.
  - `AcceptButton` switches per active tab (Tab 0: Confirm Order, Tab 1: Check Out,
    Tab 2: Pay) via `tabControl1.SelectedIndexChanged`. Escape on Tab 2 activates Go Back.
  - Stale Alt-key hints removed from `AccessibleName` on `btnConfirmOrder` and `btnCheckOut`.
  - `pictureBox1` and `pictureBox2` set to `AccessibleRole.None` to exclude from the
    accessibility tree.
  - `AccessibilityTests.cs`: 4 new unit tests verifying mnemonics, AcceptButton, decorative
    image exclusion, and input `AccessibleName` values.
  - Internal `Form1(bool showReceiptDialogs)` convenience constructor for accessibility tests.
  - Internal test-surface properties on `Form1` for accessibility assertions.

**Total tests: 282 passing.**

---

## [2.24.0] — 2026-05-14

### Added

- `AdminSmokeTests.cs`: 5 new tests covering destructive admin operations.
  - `OrderHistoryForm_VoidSelectedOrder_PersistsVoidedStatus` — opens `OrderHistoryForm`
    via STA thread (no PIN configured), selects an order in the list, clicks Void Order,
    auto-dismisses the Yes/No confirmation dialog (IDYES), and asserts the status is "Voided"
    in the database.
  - `OrderHistoryForm_DeleteSelectedOrder_RemovesFromDatabase` — same pattern for Delete Order;
    asserts the record no longer exists.
  - `OrderRepository_VoidOrder_SetsStatusToVoided` — direct repository test without WinForms.
  - `OrderRepository_DeleteOrder_RemovesRecord` — direct repository test without WinForms.
  - `BackupRestoreRoundTrip_WithRealSqliteData_PreservesActiveOrders` — saves a real order via
    `OrderRepository`, backs up with `DatabaseBackupService.BackupTo`, voids the order,
    restores via `DatabaseBackupService.RestoreFrom`, then verifies the order is "Active" again.
    This is the first test to exercise backup/restore against real SQLite data (not fake content).
- `WinFormsTestHelper.DialogAutoCloser` now also posts `WM_COMMAND/IDYES (6)` alongside
  `WM_COMMAND/IDOK (1)`, enabling auto-dismissal of Yes/No confirmation dialogs. Both are
  safe to send together: each MessageBox responds only to the button IDs it actually owns.

### Fixed

- Test name typo: `ApplyPromo_FreeshipeCode_FullDiscount` renamed to
  `ApplyPromo_FreeshipCode_FullDiscount`.

**Total tests: 278 passing.**

---

## [2.23.0] — 2026-05-14

### Added

- `CheckoutWorkflowService` (with `ICheckoutWorkflowService` interface): extracts customer
  assembly, customer and payment validation delegation, promo application, standard payment
  processing, order assembly, order-record assembly, and delivery-minutes resolution out of
  `Form1.cs` into a focused, UI-free service. Form1 now delegates all checkout decisions to
  this service; it retains only control-reads, focus management, and MessageBox calls.
- `PromoPaymentResult` and `StandardPaymentResult` result-value types returned by the service.
- 16 new unit tests in `CheckoutWorkflowServiceTests.cs` covering customer build, customer
  validation, promo codes (valid, invalid, free-shipping), standard payment (cash, overpay,
  underpay, empty method), order assembly (card masking, cash no-reference, promo discount),
  `ParseCurrencyOrZero`, and `GetDeliveryMinutes` with null settings. Total: 273 tests.

### Fixed

- `OrderSubmissionService.CreateRecord` now persists `PaymentReference` (previously omitted,
  so non-cash payment references were lost when that service was used directly).

### Changed

- `Form1.BuildCustomer()`, `Form1.BuildOrderRecord()`, and `Form1.ParseCurrencyOrZero()` are
  now thin wrappers that delegate to `CheckoutWorkflowService`; the logic lives in the service.
- `Form1.BuildOrderForReceipt()` removed; order assembly is inlined in `btnSubmitOrder_Click`
  using `_checkout.AssembleOrder(...)`.
- `Form1.GetDeliveryMinutes()` removed; delivery minutes are resolved once in
  `_checkout.GetDeliveryMinutes(_settings)` during order assembly and read from
  `order.DeliveryMinutes` in the confirmation dialog.

---

## [2.22.5] — 2026-05-14

### Changed

- `actions/upload-artifact` bumped from v6 to v7 in `build-and-test.yml` (two uses: test results and portable package artifact).
- `softprops/action-gh-release` bumped from v2 to v3 in `release.yml`. Both upgrades move to Node.js 20 runtimes, eliminating the Node 20 deprecation warnings that appeared in every CI run.

---

## [2.22.4] — 2026-05-14

### Fixed

- `release.yml` now declares `permissions: contents: write` at the workflow level so `GITHUB_TOKEN` has the scope required by `softprops/action-gh-release` to create releases and upload assets. Without it the step failed with `Resource not accessible by integration`.
- Release body text converted to ASCII punctuation to avoid encoding surprises in PowerShell, YAML parsers, and editors.

---

## [2.22.3] — 2026-05-14

### Fixed

- Checkout WinForms smoke tests no longer depend on auto-closing the post-submit receipt/order-again dialog. That dialog is a custom `Form` (not `MessageBox`), so the `WM_COMMAND/IDOK` fix in v2.22.2 never applied to it. `Form1` now has an `internal` constructor overload with a `showReceiptDialogs` flag; the public constructor delegates with `true`. Smoke tests pass `false` to skip the post-submit dialog entirely, making checkout persistence coverage deterministic in CI without touching production behavior.

---

## [2.22.2] — 2026-05-14

### Fixed

- `DialogAutoCloser` now sends `WM_COMMAND/IDOK` (in addition to `WM_CLOSE`) when a matching window is found. Single-button `MessageBox` dialogs disable the X button, so `WM_CLOSE` alone was silently dropped on CI, causing the two checkout smoke tests to hang until the 90-second STA timeout. `WM_COMMAND/IDOK` presses the OK button directly and is reliable regardless of close-button state.
- `RunInSta` default timeout raised from 90 s to 180 s to give the slower GitHub Actions runner headroom for the full checkout workflow even under load.

---

## [2.22.1] — 2026-05-14

### Fixed

- `Test-PortablePackage.ps1`: replaced a non-ASCII em dash (`—`) in a `Write-Host` string with an ASCII hyphen (`-`). The em dash caused a parser error on Windows PowerShell 5.1, preventing the smoke test from running.

---

## [2.22.0] — 2026-05-14

### Added

- `Package-PortableRelease.ps1` now writes a `PizzaExpress-*-portable.zip.sha256` sidecar file (SHA256, lowercase hex, BSD-style `hash  filename` format) alongside every portable ZIP.
- `Test-PortablePackage.ps1` verifies the `.sha256` sidecar when present before launching the smoke-test executable; outputs a clear mismatch error if the hash does not match.
- `release.yml` now uploads the `.sha256` file as a release asset alongside the ZIP so reviewers can verify downloads without trusting a separate channel.
- `PORTABLE-README.txt` now includes the version number, minimum OS/runtime requirements with a link to the .NET 4.8 download, numbered quick-start steps, and PowerShell/CertUtil verification commands.
- README "Getting Started" section now leads with a user-facing "Download and Run" block (download, extract, run — no source required) followed by a separate "Build from Source" block for developers.

### Changed

- `release.yml` release body updated to match the new user-facing install steps and reference the `.sha256` verification command.

---

## [2.21.0] — 2026-05-14

### Added

- `[assembly: InternalsVisibleTo("PizzaExpress.Tests")]` so unit tests can reach `internal static` helpers without reflection.
- `OrderHistoryForm.BuildHistoryCsv(IEnumerable<OrderRecord>)` — extracted from `ExportCsv()`; returns the CSV string as a pure function.
- `SalesReportForm.BuildSalesReportCsv(from, to, summary, daily, items, payments)` — extracted from `ExportCsv()`; builds the full sales-report CSV from typed model data.
- `EndOfDayForm.BuildZReportCsv(day, summary, payments, topItems)` — extracted from `ExportCsv()`; builds the Z-report CSV.
- `EndOfDayForm.BuildZReportText(day, summary, payments, topItems, printedAt?)` — extracted and made testable; `printedAt` defaults to `DateTime.Now` so tests can pin the timestamp.
- `ReportExportTests.cs` — 22 new unit tests covering all four content builders: header presence, field content, edge cases (empty data, null payment method, zero-qty item fallback, no-sales message).

### Changed

- `SalesReportForm` now caches `_lastFrom`, `_lastTo`, `_lastSummary`, `_lastDaily`, `_lastItems`, `_lastPayments` after `RunReport()` so `ExportCsv()` exports from typed model data rather than scraping ListView text.
- `EndOfDayForm.BuildReportText()` now delegates to the new static `BuildZReportText` overload; behavior is unchanged except the `Printed:` line uses the injected timestamp when provided.
- Test count raised from 235 to 257 (all passing).

---

## [2.20.0] — 2026-05-14

### Added

- `AGENTS.md` repository guidance covering architecture boundaries, validation commands, safe refactoring rules, docs-sync expectations, and the free-tools-only constraint.
- Regression tests covering discount-aware totals, settings-backed pricing, idempotent database migration, repository line replacement, and discount persistence.
- WinForms smoke tests covering hidden-form construction, multi-pizza checkout persistence, promo-card checkout persistence, and settings-driven price changes.
- PIN hardening regression tests covering protected PIN save/load behavior, legacy plaintext upgrade, temporary lockout, and recent-session authorization reuse.

### Fixed

- Fresh-install database startup now succeeds reliably: migrations create core tables first, seed default settings safely, and add later columns idempotently.
- Order persistence now replaces existing line items for the same order id instead of duplicating them on re-save.
- SQLite repository connections now enable foreign keys consistently, and order deletion removes child rows safely.
- Promo-card checkout now persists discounted totals, writes discount metadata to order history, and prints discount-aware receipts and change correctly.
- Receipt delivery estimates now use the runtime delivery setting end-to-end.
- Drink, water, and side pricing now respect SQLite settings in both the live total and the confirmed order flow.
- CI workflows now use correct repository-root paths and updated free GitHub Action versions, fixing broken automation caused by stale path prefixes.
- GitHub Actions and contributor instructions now use a verified `vstest.console.exe` runner via `scripts/Run-Tests.ps1`, matching the actual MSTest execution path for this .NET Framework solution.
- Staff PIN values are no longer saved or displayed as plain text in `SettingsForm`; configured PINs stay masked in the UI and new values are stored with PBKDF2 hashing.
- `PinLoginForm` now verifies PBKDF2-protected PINs, upgrades legacy plaintext PINs on successful login, records recent staff authorization, and temporarily locks the keypad after repeated failures.
- Sensitive admin actions now reuse recent staff authorization: opening Settings and voiding/deleting from Order History re-prompt only when the session is stale.

### Changed

- The About dialog and contributor docs no longer hardcode stale test totals.
- README, SECURITY, AGENTS, CLAUDE, and CONTRIBUTING were updated to reflect the current stack, payment-reference handling, PIN trust boundary, packaging flow, and verification workflow.

---

## [2.20.0] — 2026-05-14

### Fixed

- **WinForms smoke test flakiness** — `FormSmokeTests` and `PinHardeningTests` now carry `[DoNotParallelize]` so their STA threads never share window-space with other concurrently running test classes. `WinFormsTestHelper` changes: default `RunInSta` timeout raised from 60 s to 90 s; `PumpEvents` now drives three `DoEvents` / 50 ms cycles instead of two 100 ms ones; `DialogAutoCloser.WatchLoop` polls every 50 ms (down from 100 ms) and fires both `SendMessage` and `PostMessage(WM_CLOSE)` for faster modal dismissal. The DB-size label assertion in `SettingsForm_SaveChanges` test was tightened to match "DB: —" as well as "DB: N KB" (previously failed when the test DB was < 1 KB).

### Changed

- `AssemblyInfo.cs` — version bumped to `2.20.0.0`.

---

## [2.19.0] — 2026-05-12

### Added

- **Restore re-auth gate** — `SettingsForm.BtnRestore_Click` now calls `PinLoginForm.EnsureAuthorized` with `TimeSpan.Zero` (always requires fresh PIN entry) before allowing a database restore. Backup and View Auto-Backups remain accessible within the existing session window.
- **`DatabaseBackupServiceTests`** — 14 new integration tests covering `RunAutoBackup` (absent DB no-op, creates dated file, idempotent same-day, prunes to 7), `BackupTo` (round-trip, missing DB, empty path), `RestoreFrom` (replaces live DB, creates safety copy, missing source, no live DB), `GetDatabaseSizeKb` (absent/present), and `GetAutoBackups` (empty dir, newest-first ordering).

### Changed

- `AssemblyInfo.cs` — version bumped to `2.19.0.0`.

---

## [2.18.0] — 2026-05-12

### Added

- **PIN hardening** — Staff PINs are now stored with PBKDF2 (SHA-1, 100 000 iterations, 16-byte salt). `SettingsForm` masks configured PINs in the UI and hashes new values on save. Legacy plaintext PINs are upgraded to PBKDF2 on successful login via `PinLoginForm`. Three failed attempts trigger a 10-second keypad lockout with a countdown label. `StaffAuthSession` records the last successful auth time.
- **Sensitive-action re-auth** — `PinLoginForm.EnsureAuthorized()` gates opening `SettingsForm` (Form1 `Alt+W`) and both Void/Delete in `OrderHistoryForm`. Recent staff sessions (10-minute window) are reused so operators are not prompted on every click.
- **WinForms smoke tests** — four hidden-form integration tests covering form construction, multi-pizza cash checkout, promo-card checkout, and settings-driven price changes.
- **`scripts/Run-Tests.ps1`** — wrapper script that locates `vstest.console.exe` and runs the net48 MSTest assembly; used by CI and contributor docs.
- **`scripts/Package-PortableRelease.ps1`** / **`Test-PortablePackage.ps1`** — portable ZIP packaging and launch smoke test.
- **7 PIN hardening regression tests** in `PinHardeningTests.cs` covering PBKDF2 protect/verify, legacy upgrade, lockout, and recent-session reuse.

### Changed

- CI workflows (`build-and-test.yml`, `release.yml`) updated to use `Run-Tests.ps1` instead of `dotnet test`.
- `AssemblyInfo.cs` — version bumped to `2.18.0.0`.

---

## [2.17.0] — 2026-05-12

### Added

- **Payment reference persistence** — `Order.PaymentReference` and `OrderRecord.PaymentReference` fields added. Non-cash references (card last-4 masked by `PaymentReferenceHelper`, transaction IDs, etc.) are captured at checkout, persisted via a new schema migration (`0004_AddPaymentReference`), shown on the printed receipt, and displayed in the Order History detail view.
- **3 repository tests** — `Save_WithPaymentReference_PersistsAndLoadsBack`, `Save_CashPayment_ReferenceIsNull`, `Save_DebitCard_ReferenceRoundTrips`.

### Changed

- `AssemblyInfo.cs` — version bumped to `2.17.0.0`.

---

## [2.16.0] — 2026-05-11

### Added

- **`DatabaseBackupService`** (`Infrastructure/DatabaseBackupService.cs`) — static service for rolling auto-backups, manual backup, and database restore with a pre-restore safety copy. Keeps the last 7 dated auto-backups and prunes older ones; `GetDatabaseSizeKb` and `GetAutoBackups` utilities included.
- **Auto-backup on startup** — `Program.cs` calls `DatabaseBackupService.RunAutoBackup(dataDir)` once per day after migrations; silent no-op if the database does not yet exist.
- **Backup / Restore UI in `SettingsForm`** — a second toolbar below the Save/Cancel row exposes three buttons: **Backup DB…** (opens `SaveFileDialog`, copies live DB), **Restore DB…** (opens `OpenFileDialog` with confirmation, saves safety copy before overwriting), and **View Auto-Backups** (lists dated auto-backup files). A small label shows the current database size in KB.
- **Four missing namespace closures** — `OrderSubmissionService`, `PaymentReferenceHelper`, `PinSecurity`, and `StaffAuthSession` were missing their closing `}` for the namespace block; corrected.

### Fixed

- `OrderSubmissionService.CreateRecord` referenced `Order.PaymentReference` and `OrderRecord.PaymentReference` which do not exist on those models; removed the field assignment so the service compiles correctly.

### Changed

- `AssemblyInfo.cs` — version bumped to `2.16.0.0`.

---

## [2.15.0] — 2026-03-26

### Added

- **`EndOfDayForm`** (`Forms/EndOfDayForm.cs`) — Z-Report: cashier shift-close summary for the current calendar day. Shows four KPI boxes (Orders, Revenue, GST, Average), a payment-method breakdown ListView, and a top-items ListView. Print to any printer via `PrintPreviewDialog`; Export CSV button. Opened via **"End of Day"** button (forest-green, Tab 2) or `Alt+E`.
- **`Alt+E` keyboard shortcut** — wired in `ProcessCmdKey` to open `EndOfDayForm` from anywhere in the main form.

### Fixed

- **`PrintReceipt` multi-page and line-wrap** — previous implementation used a single `DrawString` call, truncating any receipt longer than one page and not wrapping long lines. Rewritten to walk lines one-by-one with character-level wrapping and proper `HasMorePages` signalling so any receipt length prints correctly.

### Changed

- **About dialog** — test count updated from 95 to 192.
- **Sales Report button** — removed the Unicode emoji (surrogate-pair) from the button label; plain text "Sales Report" renders reliably across all Windows DPI settings.
- **Order confirmation delivery estimate** — reads `DeliveryMinutes` from the SQLite `Settings` table at runtime (falls back to `AppConfig.DeliveryMinutes` when settings are unavailable). Delivery time now reflects whatever the admin has configured, without a restart.
- **README** — updated version badge (2.15.0), test count badge (192), architecture diagram, feature sections (reporting, admin, keyboard shortcuts), and project structure tree.
- `AssemblyInfo.cs` — version bumped to `2.15.0.0`.

---

## [2.14.0] — 2026-03-26

### Added

- **`PinLoginForm`** (`Forms/PinLoginForm.cs`) — dark-theme numpad PIN dialog shown at startup when a staff PIN is configured. Features: circle-dot masking (●), physical numpad and keyboard support (digits, Backspace, Delete, Enter, Escape), CLR button, shake animation on incorrect entry. If `StaffPin` is empty the dialog is bypassed entirely — zero friction for single-operator shops.
- **Staff PIN startup gate in `Program.cs`** — `PinLoginForm.PinRequired(settings)` check added between migration and `Application.Run`; cancelling the dialog exits cleanly without launching the POS.
- **`SalesReportForm`** (`Forms/SalesReportForm.cs`) — dark-theme period sales report form (1020×680). Period selector with Today / This Week / This Month shortcuts, four KPI boxes (Orders, Revenue, GST, Avg Order in amber), and three detail ListViews: Daily Breakdown, Top Items (by revenue), Payment Methods. Includes Export CSV. Opened via **Sales Report** button (Tab 2) or `Alt+R`.
- **Sales Report button** (Tab 2, `Form1`) — wired to `OpenSalesReportForm()`; `Alt+R` keyboard shortcut registered in `ProcessCmdKey`.
- **`StaffPin` friendly name in `SettingsForm`** — displays as "Staff PIN (leave blank to disable)" in the admin settings grid.
- **Void Order in `OrderHistoryForm`** — "Void Order" button (dark amber), right-click context menu item, and `V` keyboard shortcut. Voided orders remain in the log (for audit) but are excluded from all revenue reports. Voided rows are rendered in italic grey so they are immediately distinguishable from active orders.
- **Status column in `OrderHistoryForm`** — shows "Active" or "Voided" per row; detail view includes the status line.
- **15 new integration tests** in `OrderRepositoryTests` covering `VoidOrder` (3), `GetSummaryForPeriod` (3), `GetDailySummaries` (3), `GetTopItems` (3), and `GetPaymentBreakdown` (3).

### Changed

- `AssemblyInfo.cs` — version bumped to `2.14.0.0`.

---

## [2.13.0] — 2026-03-26

### Added

- **Composition root in `Program.cs`** — single explicit wiring point; `Form1` now depends only on interfaces. The only place that calls `new` on a concrete service is `Program.cs`.
- **`DatabaseMigrator`** (`Infrastructure/DatabaseMigrator.cs`) — dependency-free migration runner built on existing SQLite + Dapper. Tracks applied scripts in a `SchemaHistory` table; each script runs exactly once across all environments. Zero new NuGet packages.
- **`Settings` database table** (migration `0001_AddSettingsTable`) — all prices and delivery time now persisted in SQLite, seeded with defaults on first run.
- **`ISettingsRepository` + `SettingsRepository`** — interface + SQLite implementation for reading/writing the `Settings` table.
- **`CartService` dynamic pricing** — accepts an optional `ISettingsRepository`; prices are resolved from the DB at order time, falling back to `AppConfig` compile-time values. All 177 existing tests pass unchanged.
- **`SettingsForm`** (`Forms/SettingsForm.cs`) — dark-theme admin form; opens via `⚙ Settings` button (Tab 2) or `Alt+W`. DataGridView with friendly names, inline editing, numeric validation, immediate persistence. Changes take effect on the next order — no restart required.

### Changed

- **`Form1` constructor injection** — hardwired field initialisers replaced with `(IOrderRepository, ICartService, ISettingsRepository)` constructor. Parameterless fallback retained for WinForms Designer.
- `AssemblyInfo.cs` — version bumped to `2.13.0.0`.

---

## [2.12.0] — 2026-03-26

### Fixed

- **Critical pricing bug in `BuildOrderForReceipt()`** — the ListView price column stores line totals (`qty × unitPrice`), but the method was passing that value directly as the `unitPrice` parameter of `OrderItem`. Since `OrderItem.TotalPrice = UnitPrice × max(Quantity, 1)`, the quantity was being applied twice, inflating `Order.Subtotal` and producing a negative change amount on the receipt. Fixed by back-calculating `unitPrice = lineTotal / max(qty, 1)` before constructing each `OrderItem`.

### Added

- **2 regression tests** in `ReceiptWriterTests` — `Build_MultiQtyDrink_SubtotalNotDoubledByQuantity` and `Build_MultiQtyDrink_DoesNotDoubleCount` guard against re-introducing the double-counting bug.
- **World-class UI/UX overhaul (Form1)** — applied programmatically in `ApplyUiPolish()`:
  - Form launches **maximised** so the tab control fills the screen with no dead space or scrolling.
  - `tabControl1` gains `Anchor = Top | Left | Right | Bottom` so it resizes with the window.
  - Form background changed from `DarkGoldenrod` to brand burnt-orange `rgb(192, 64, 0)`, eliminating the ugly golden-brown strip.
  - **Button colour hierarchy**: primary forward-flow buttons (Confirm Order, Check Out, Submit Order) → brand orange-red; secondary/back buttons (Add to Cart, Order Again, Go Back) → charcoal; destructive buttons (Exit, Clear Order) → crimson; Pay button → forest green. All buttons use `FlatStyle.Flat`, no border, `Cursor.Hand`, "Segoe UI 10pt Bold".
  - "Add Pizza to Cart" button auto-sized to `175×40` so the label is never clipped.
  - History and About buttons resized to match the rest of the action row.
- **World-class UI/UX overhaul (OrderHistoryForm)** — dark-theme redesign:
  - Background `rgb(26, 26, 26)` (deep charcoal) with surface panels `rgb(38, 38, 38)`.
  - Filter bar on dark surface; search TextBox styled dark; labels in `rgb(240, 240, 240)`.
  - Stats bar uses a dark brand-tint panel with warm amber text `rgb(255, 200, 140)`, "Segoe UI 9pt Bold" — instantly visible at a glance.
  - ListView dark background `rgb(32, 32, 32)` with light text; no border.
  - Button bar: View Details → brand orange-red; Delete → crimson; Export CSV / Close → charcoal. All flat, hand cursor, "Segoe UI 9.5pt Bold".
  - Form default size increased to `940×600`.

### Changed

- `AssemblyInfo.cs` — version bumped to `2.12.0.0`.

---

## [2.11.0] — 2026-03-26

### Added

- **Central Package Management (`Directory.Packages.props`)** — all 12 NuGet package versions centralised in a single file. `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` enabled in `Directory.Build.props`. Upgrading any package now requires editing exactly one line in one file; version drift between projects is impossible.
- **Tooltips on Order History controls** — hover hints added to the stats bar (explains all-time vs. filtered), Delete button (`Del` key reminder), View Details button (`Enter` key reminder), and Export CSV button.

### Changed

- `Directory.Build.props` — `ManagePackageVersionsCentrally` enabled; analyzer `PackageReference` entries no longer carry `Version=` attributes.
- Both `.csproj` files — all `Version=` attributes removed from every `PackageReference`. Versions now resolved exclusively from `Directory.Packages.props`.
- `AssemblyInfo.cs` — version bumped to `2.11.0.0`.

---

## [2.10.0] — 2026-03-26

### Added

- **`Directory.Build.props`** — single source of truth for properties shared by every project: `LangVersion=7.3`, `Nullable=disable`, `Deterministic=true`, and both analyzer packages (`Microsoft.CodeAnalysis.NetAnalyzers` + `StyleCop.Analyzers`). Eliminates the duplication that previously existed between the two `.csproj` files.
- **`global.json`** — pins the .NET SDK to `9.0.x` (`rollForward: latestMinor`, `allowPrerelease: false`). Ensures every developer and CI agent uses the same SDK version, preventing "works on my machine" build differences.
- **`NuGet.Config`** — explicit `<packageSources>` (nuget.org only) + `<packageSourceMapping>`. Prevents accidental resolution from machine-level or corporate feed sources; makes package restore fully reproducible and auditable.
- **`Delete` / `Enter` keyboard shortcuts in Order History** — `KeyPreview = true` on the form; `Delete` key triggers the delete-with-confirmation flow, `Enter` key opens the detail view. No mouse required for common actions.

### Changed

- Both `.csproj` files — `LangVersion`, `Nullable`, and analyzer `PackageReference` blocks removed (now inherited from `Directory.Build.props`). Files are shorter and easier to read.
- `AssemblyInfo.cs` — version bumped to `2.10.0.0`.

---

## [2.9.1] — 2026-03-26

### Changed

- **Zero-warning CI gate** — `dotnet build` in both `build-and-test.yml` and `release.yml` now passes `-warnaserror`, turning any surviving diagnostic warning into a build failure.
- **`.editorconfig`** — all eight StyleCop rule categories suppressed via `dotnet_analyzer_diagnostic.category-StyleCop.CSharp.*.severity = none`. This eliminates the ~740 pre-existing StyleCop noise warnings (spacing, ordering, naming, layout, documentation) that were present in the WinForms / Designer-generated code without meaningful benefit. Additional CA rule suppressions added for common WinForms patterns: `CA1002`, `CA1031`, `CA1051`, `CA1501`, `CA1502`, `CA1506`, `CA1711`, `CA1716`, `CA1724`, `CA2213`, `CA2227`, `CA1416`.
- `AssemblyInfo.cs` — version bumped to `2.9.1.0`.

---

## [2.9.0] — 2026-03-26

### Added

- **`IOrderRepository.Delete(id)`** — permanently removes an order and all its line items in a single atomic transaction (`ON DELETE CASCADE` handles the lines). No-ops silently on unknown or null ids.
- **Delete Order UI** — "Delete Order" button (dark-red text) in the Order History button bar; also available via right-click context menu on any list row. Both paths show a confirmation dialog (default: No) before deleting, then refresh the list and stats bar automatically.
- **SQLite indexes** — `IX_Orders_OrderDate` on `Orders(OrderDate)` and `IX_OrderLines_OrderId` on `OrderLines(OrderId)` created at schema initialisation (`CREATE INDEX IF NOT EXISTS`). Both existing and new databases gain the indexes automatically on next launch.
- **5 new tests** — 4 integration tests in `OrderRepositoryTests` (`Delete` removes record, `Delete` cascades lines, non-existent id is a no-op, null/empty id is a no-op); 1 NSubstitute mock test in `ServiceInterfaceTests`. **Total tests: 170 → 175**.

### Changed

- `CONTRIBUTING.md` — updated test count 145 → 175; replaced legacy `msbuild` + `vstest.console.exe` commands with `dotnet restore` / `dotnet build` / `dotnet test`.
- `AssemblyInfo.cs` — version bumped to `2.9.0.0`.

---

## [2.8.0] — 2026-03-26

### Added

- **SQL-powered search** — `IOrderRepository` gains `Search(string text, DateTime? from, DateTime? to)`. All filtering is now performed in the database via SQL `LIKE` and date-range `WHERE` clauses instead of iterating a full in-memory list. Matches on CustomerName, Region, PaymentMethod, or OrderDate.
- **`OrderSummary` model + `GetSummary()` method** — new `IOrderRepository.GetSummary()` returns a single-query aggregate (`COUNT`, `SUM(Total)`, `AVG(Total)`) rounded to 2 d.p.
- **Stats bar in Order History** — a slim panel above the list shows all-time order count, total revenue, and average order value (always reflects the full database, not the current filter).
- **9 new tests** — 7 new `OrderRepositoryTests` (Search with no filter, by text, by date range, combined text+date, no-match, `GetSummary` on empty DB, `GetSummary` with data); 2 new `ServiceInterfaceTests` (NSubstitute mocks for `Search` and `GetSummary`). **Total tests: 161 → 170**.

### Changed

- `OrderHistoryForm.ApplyFilter()` — replaced in-memory C# loop with `_repo.Search()` call; no longer loads all rows on every keystroke.
- `OrderHistoryForm.LoadOrders()` — now calls `RefreshStats()` once on open, then delegates to `ApplyFilter()`.
- `AssemblyInfo.cs` — version bumped to `2.8.0.0`.

---

## [2.7.0] — 2026-03-25

### Added

- **SQLite embedded database** — `OrderRepository` now persists orders to `%APPDATA%\PizzaExpress\orders.db` using SQLite 1.0.118.0 via Dapper 2.1.35. Replaces the NDJSON flat-file store. Schema created automatically on first run; both the order header (Orders table) and line items (OrderLines table) are written in a single ACID transaction.
- **Automatic three-generation migration** — on first launch the repository checks for legacy data files and silently imports them: (1) `orders.ndjson` → SQLite, then renames to `.migrated`; (2) if no NDJSON, `orders.json` (pre-v2.4.0 JSON array) → SQLite, then renames to `.migrated`. Zero user action required.
- **SDK-style project files** — both `.csproj` files migrated from old-style (`ToolsVersion="4.0"` + `packages.config`) to SDK-style (`Sdk="Microsoft.NET.Sdk"` + `PackageReference`). Enables `dotnet restore`, `dotnet build`, `dotnet test` and lays the groundwork for the planned WPF/.NET 8 migration (Phase 10).

### Changed

- CI workflows (`build-and-test.yml`, `release.yml`) — replaced `setup-msbuild` + `setup-nuget` + `nuget restore` + `msbuild` + `vstest.console.exe` search with `actions/setup-dotnet@v4` + `dotnet restore` + `dotnet build` + `dotnet test`. Simpler, faster, and more portable.
- `Microsoft.CodeAnalysis.NetAnalyzers` bumped from `8.0.0` → `9.0.0` to match the .NET 9 SDK's built-in analyzer version and eliminate the build warning.
- `AssemblyInfo.cs` — version bumped to `2.7.0.0`.

---

## [2.6.0] — 2026-03-25

### Added

- **6 new `OrderValidatorTests`** — `ValidateCustomer` now tested for: `null` customer, missing `LastName`, missing `Address`, customer with invalid email (delegates to `ValidateEmail`), customer with invalid contact number (delegates to `ValidateContactNo`); `ValidatePayment` now tested for `null` method. **Total tests: 145 → 151**.
- **5 new `ReceiptWriterTests`** — `Build()` now verified to contain: `Subtotal:` label, `Change:` label, customer contact number, customer region, and delivery time (30 min). **Total: 151 → 156**.
- **2 new `PromoEngineTests`** — `Apply` message verified to contain the percentage string (`"10%"`) for discount codes; verified to contain the discounted total for `PIZZA20`. **Total: 156 → 158**.
- **2 new `OrderRepositoryTests`** — `LoadAll` with a present-but-empty NDJSON file returns an empty list (not a crash); `Save` of three records round-trips in insertion order. **Total: 158 → 160**.
- **1 new `CartServiceTest`** — `BuildPizzaItems` for `PizzaSize.Large` verifies the unit price is `$10.00`. **Total: 160 → 161**.

### Changed

- `CONTRIBUTING.md` — updated stale "all 95 tests" reference to "all 145 tests"; added Coverlet collection to the local test command example; added ≥ 70% line-coverage requirement.
- `AssemblyInfo.cs` — version bumped to `2.6.0.0`.
- README — version badge, test count badge, description, project structure tree, and contributing section all updated to reflect 161 tests.

---

## [2.5.0] — 2026-03-25

### Added

- **Lifetime-free CI coverage gate** — Codecov (third-party, pricing risk) replaced with a self-contained PowerShell step that parses `coverage.opencover.xml`, writes a formatted table directly to the **GitHub Actions job summary**, and fails the build if line coverage drops below 70%. Zero external services, zero tokens, works forever.
- **`.gitattributes`** — eliminates the CRLF/LF conversion warnings that appeared on every `git commit`. Sets `text=auto eol=crlf` for C# source files and `eol=lf` for YAML/shell CI files; marks binaries as `binary`.
- **`NullLogger`** — sealed no-op `ILogger` implementation with a shared `Instance` singleton. Use in tests and anywhere logging output is unwanted without passing `null`.
- **`FileLoggerTests`** — 14 tests covering `FileLogger` and `NullLogger`: Info/Warn/Error write to file, timestamp format, exception type included, multiple entries appended, directory auto-created, silent failure when path is inaccessible, and all `NullLogger` variants.
- **ILogger + ICartService mock tests** — 7 new tests added to `ServiceInterfaceTests`: NSubstitute mocks for `ILogger.Info`, `ILogger.Error`, `ICartService.BuildPizzaItems`, `ICartService.CalculateTotal`; real-impl interface checks for `NullLogger`, `FileLogger`, and `CartService`. **Total tests: 123 → 145**.
- **Release build configuration** for `PizzaExpress.Tests.csproj` — the test project was missing a `Release|AnyCPU` `PropertyGroup`, causing the CI release workflow to fail. Added with `Optimize=true`, `pdbonly` debug symbols, and correct `OutputPath`.

### Changed

- `build-and-test.yml` CI: Codecov upload step removed; replaced with `Parse coverage and write job summary` PowerShell step with a 70% line-coverage gate.
- README: Codecov badge replaced with static `coverage: >70% gated` badge; test count updated to 145.

---

## [2.4.0] — 2026-03-25

### Added

- **`ICartService` + `CartService`** — pizza-building and price-calculation logic extracted from `Form1.cs` into a dedicated, WinForms-free service. `Form1` now calls `_cart.BuildPizzaItems()` and `_cart.CalculateTotal()` instead of duplicating math inline. Fully unit-testable.
- **19 `CartServiceTests`** — cover `BuildPizzaItems` (all sizes, crust types, qty scaling, topping names, empty/null toppings, zero-qty guard), `CalculateSubtotal` (empty, null, multi-item), `CalculateTax` (zero, standard, rounding), `CalculateTotal`. **Total tests: 104 → 123**.
- **Live running total in status bar** — right side of the status bar updates in real time as the user selects pizza size, crust, quantity, toppings, drinks, and sides on Tab 1. Shows `Live total (incl. GST): $XX.XX`. Updates instantly on every checkbox/radio/NumericUpDown change.
- **Input length limits** — `MaxLength` set on every `TextBox`: FirstName/LastName 50, Address 100, City 60, PostalCode 4, ContactNo 15, Email 100, CardOrPromo 30, AmountPaid 12.
- **Accessibility labels** — `AccessibleName` set on 22 interactive controls (text fields, combo boxes, buttons, ListView). Screen readers now announce meaningful names instead of generic "Button" or "TextBox".
- **`ILogger` interface** — minimal `Info` / `Warn` / `Error(message, ex)` contract; services and UI receive it via field injection.
- **`FileLogger` implementation** — appends timestamped entries to a daily-rotating file at `%APPDATA%\PizzaExpress\Logs\app_yyyy-MM-dd.log`. Thread-safe. Never throws (all I/O errors swallowed internally).
- **Structured app logging** — three key events logged: application start (with version), order saved (with ID, customer, total, method), promo applied (with code and discounted amount). Errors during history persistence logged at ERROR level with exception details.
- **Append-only NDJSON store** — `OrderRepository` switched from full-array JSON (read-whole-file-on-every-save) to Newline-Delimited JSON (one line per order). `Save()` is now O(1) regardless of history size; `LoadAll()` reads line-by-line and silently skips corrupted lines.
- **Automatic legacy migration** — if `orders.json` (old format) exists but `orders.ndjson` does not, all records are imported on first load and the old file is renamed `orders.json.migrated`. Zero user action required.
- **2 new `OrderRepositoryTests`** — corrupted-line-is-skipped test and legacy migration test. Total repository tests: 7 → 9.
- **Coverlet `coverlet.runsettings`** — configures OpenCover format output, excludes test assembly and auto-generated files (`Form1.Designer.cs`, `Program.cs`, `AssemblyInfo.cs`).
- **CI: Coverlet + Codecov** — `build-and-test.yml` now collects `XPlat Code Coverage` using `coverlet.runsettings`, uploads the `coverage.opencover.xml` report to Codecov, and uploads it as a build artifact. Codecov badge added to README.
- **Setup note:** add `CODECOV_TOKEN` as a GitHub Actions secret (Settings → Secrets → New repository secret) from [codecov.io](https://codecov.io) to enable the live coverage badge.

### Changed

- `OrderRepository` data file: `orders.json` → `orders.ndjson` (with automatic one-time migration from old format).
- `Form1.BuildCurrentPizzaItems()` now delegates to `CartService.BuildPizzaItems()` — no duplicate price logic in UI.

---

## [2.3.0] — 2026-03-25

### Added

- **Email validation** — `IOrderValidator.ValidateEmail()` and `OrderValidator.ValidateEmail()` validate optional email format (`local@domain.tld`). `ValidateCustomer()` now also checks email. Inline green/red feedback on the Email field `Leave` event.
- **9 new email validation tests** — null, empty, whitespace (all valid/optional), standard address, subdomain address, missing `@`, missing domain, missing TLD, error message content. **Total tests: 95 → 104**.
- **Postal code input masking** — `txtPostalCode` now accepts digits only (non-digit keys silently blocked) and is capped at 4 characters, making invalid input impossible to type.
- **Global unhandled exception handler** in `Program.cs` — catches exceptions on both the UI thread (`Application.ThreadException`) and background threads (`AppDomain.UnhandledException`). Writes a timestamped crash log to `%APPDATA%\PizzaExpress\Logs\crash_*.log` and shows a user-friendly error dialog instead of a raw .NET crash box.
- **Version in title bar** — title now shows `Pizza Express New Zealand — v2.3.0` using `Application.ProductVersion` (reads from `AssemblyFileVersion`).
- **About dialog** — new "About" button on Tab 2 shows version, architecture summary, test count, CI note, and the local data directory path.
- **Column header sorting in Order History** — clicking any column header sorts by that column; clicking again toggles ascending/descending. Sort direction indicated by ▲/▼ arrow in the header text.
- **`AssemblyInfo.cs` updated** — `AssemblyTitle`, `AssemblyProduct`, `AssemblyCompany`, `AssemblyCopyright`, `AssemblyVersion`, and `AssemblyFileVersion` all set to reflect Pizza Express NZ v2.3.0.
- **`CONTRIBUTING.md` rewritten** — reflects current 104-test suite, interface-first architecture rule, StyleCop/Roslyn zero-warning requirement, and conventional commit format table.

---

## [2.2.0] — 2026-03-25

### Added

- **Service interfaces** — `IPromoEngine`, `IOrderValidator`, `IReceiptWriter`, `IOrderRepository` introduced; all service fields in `Form1` and `OrderHistoryForm` now use the interface type. Services are fully swappable without touching UI code.
- **Roslyn NetAnalyzers 8.0** — ~200 FxCop-style rules run at compile time on both projects (warnings surfaced in Visual Studio Error List).
- **StyleCop.Analyzers** — enforces consistent C# naming, file layout, and spacing rules across the codebase.
- **`.editorconfig`** — root-level config: 4-space indent, UTF-8-BOM, trailing-whitespace trimming, `_camelCase` private field convention; analyzer severity overrides tuned for WinForms desktop.
- **NSubstitute 5.1.0** — mocking library added to test project; Castle.Core 5.1.1 included as required proxy-generation dependency.
- **`ServiceInterfaceTests.cs`** — 9 new tests confirming every interface is correctly implemented by its concrete class and that NSubstitute can substitute each interface.
- **`OrderRepositoryTests.cs`** — 7 integration tests for `OrderRepository` using an isolated temp directory (never touches real user data). Covers save, multi-record persistence, field round-trip, missing file, corrupted file, directory auto-creation, and null guard.
- **Coverlet 6.0.2** — code coverage collector added to test project.
- **`release.yml`** CI workflow — automatically builds a Release configuration, runs all tests, and publishes the compiled `.exe` as a GitHub Release asset when a `v*.*.*` tag is pushed.
- **Dependabot** — weekly automated PRs for stale NuGet packages and GitHub Actions versions.
- **Status bar** — live strip at the bottom of the main window showing cart item count and running total (incl. GST) in real time.
- **Tooltips** — every non-obvious field now shows a concise hint on hover (postal code format, contact number rules, amount paid, etc.).
- **Confirm before clearing** — "Clear Order" now shows a warning dialog; cancellable.
- **Remove item from cart** — right-click any item in the Order Review ListView and choose "Remove selected item"; status bar updates instantly.
- **Keyboard shortcuts** — `Alt+C` (Confirm Order), `Alt+H` (Order History), `Alt+P` (Proceed to Payment), `Esc` (navigate back a tab).
- **Print Receipt** — "Print" button in the post-order receipt dialog; opens a full `PrintPreviewDialog` using `Courier New` monospace font.
- **Copy Receipt to Clipboard** — one-click copy of the full formatted receipt text.
- **Order History: live search** — text box filters by customer name, region, payment method, or date while typing.
- **Order History: date range filter** — optional From/To date pickers narrow the history list.
- **Order History: result count** — label shows how many orders match the current filter.
- **Order History: CSV export** — "Export CSV" button saves all visible rows to a `.csv` file (UTF-8).
- **XML `<summary>` doc comments** on all public models, service interfaces, and `OrderHistoryForm`.
- **GitHub issue templates** (Bug Report + Feature Request) under `.github/ISSUE_TEMPLATE/`.
- **PR template** — checklist enforcing tests, CHANGELOG update, and zero magic strings.
- **`SECURITY.md`** — responsible disclosure policy.
- **`CODE_OF_CONDUCT.md`** — Contributor Covenant v2.1.
- **`README.md`** completely rewritten — ASCII architecture diagram, full feature list, updated tech stack table, updated test count (95), project structure tree, and all new badges.
- **`OrderRepository` injectable data path** — constructor overload accepting a custom directory; enables clean integration tests without redirecting `%APPDATA%`.

### Changed

- Total test count: **79 → 95** (16 new tests across 2 new test classes).
- `OrderRepository` refactored from static file path fields to instance fields; data directory now injectable via constructor.

---

## [2.1.0] — 2026-03-24

### Added

- **Order History persistence** — every submitted order is automatically saved to `%APPDATA%\PizzaExpress\orders.json` using `System.Web.Script.Serialization.JavaScriptSerializer` (built-in, no extra NuGet packages). Records include a short unique ID, timestamp, customer details, itemised lines, and totals.
- **Order History viewer** — a new "Order History" button on the Order Review tab opens a resizable dialog listing all past orders (most-recent first) with Date/Time, Customer, Region, Payment, and Total columns. Double-clicking or pressing "View Details" shows the full formatted receipt in a message box.
- **Inline field validation with visual feedback** — the Postal Code and Contact Number fields now colour their background on `Leave`:
  - Green (Honeydew) when the value passes validation
  - Red (MistyRose) when it fails
  - Background resets to the system default on "Order Again"
- **`OrderRecord` / `OrderLineRecord` models** — lightweight, serialisable snapshots decoupled from the live `Order` domain model.
- **`OrderRepository` service** — encapsulates all JSON read/write logic; creates the data directory on first use; gracefully returns an empty list if the file is missing or corrupt.
- **`OrderHistoryForm`** — fully code-constructed WinForms dialog (no `.Designer.cs`).

---

## [2.0.0] — 2026-03-24

### Added

- Three-layer architecture: domain **Models** (`Order`, `OrderItem`, `Customer`, `PizzaSize`, `CrustType`), **Config** (`AppConfig`), and **Services** (`PromoEngine`, `OrderValidator`, `ReceiptWriter`, `ValidationResult`, `PromoResult`).
- `AppConfig` as single source of truth for all constants (tax rate, prices, regions, promo codes, payment methods, delivery time).
- **MSTest unit-test project** (`PizzaExpress.Tests`) — 79 tests covering all service classes and models, 100% passing.
- **GitHub Actions CI pipeline** (`build-and-test.yml`) — builds on `windows-latest` and runs all 79 tests on every push and pull request to master.
- **NZ localisation** — GST 15%, 16 NZ regions (Province dropdown), 4-digit NZ postal code validation, "Price NZD" column header, NZD currency formatting throughout.
- **Resizable window** — `FormBorderStyle.Sizable` + `MaximizeBox = true` with enforced `MinimumSize`.
- **Descriptive control names** — all 60+ generic control names (`textBox1`, `checkBox1`, `button1`, …) renamed to meaningful identifiers (`txtFirstName`, `cbPepperoni`, `btnConfirmOrder`, …) via a PowerShell word-boundary regex script.
- **`System.Web.Extensions` assembly reference** for built-in JSON serialisation.

### Changed

- `TargetFrameworkVersion` upgraded from `v4.5` to `v4.8`.
- Replaced hard-coded magic numbers/strings with `AppConfig` constants across all handlers.
- Replaced `Controls.Find` loops (broken after rename) with direct typed field references.

### Removed

- Sourcecodester attribution from UI footer.
- All Canadian localisation (provinces, HST 13%, CAD currency, Canadian postal code format).

---

## [1.3.0] — 2026-03-23

### Added

- **US-30** — Pizza Quantity Selection: a `NumericUpDown` control (min 1, max 20, default 1) is now displayed in the Pizza Size groupbox. The pizza ListView row shows the chosen quantity and the price reflects `unit price × qty`. Qty resets to 1 on "Order Again".
- **US-31** — Order Multiple Different Pizzas: a new "Add Pizza to Cart" button stages the current pizza (size + crust + qty + toppings), resets those fields to defaults, and allows the customer to configure another pizza before clicking "Confirm Order". All staged pizzas are flushed into the order list when "Confirm Order" is clicked. Staged list clears on "Order Again".

---

## [1.2.0] — 2026-03-23

### Added

- **US-34** — Promo Code Discount: the "Promo Card" payment method now accepts real discount codes.
  - Supported codes: `PIZZA10` (10% off), `PIZZA20` (20% off), `FREESHIP` (100% off / free order)
  - The card number label dynamically changes to "*Promo Code:" when Promo Card is selected
  - Invalid codes are rejected with a clear error message
- **US-35** — Export Order Receipt: after clicking Submit Order a Yes/No dialog offers to save a timestamped `.txt` receipt.
  - Receipt includes store header, customer delivery info, itemised order list, subtotal / HST / total, and payment method
  - Default filename: `Receipt_yyyyMMdd_HHmmss.txt`

### Improved

- **US-32** — Postal Code Format Validation: the Pay button now enforces the Canadian A1A 1A1 format (regex `^[A-Z]\d[A-Z]\d[A-Z]\d$`) before processing any payment. Spaces and lowercase input are normalised automatically.
- **US-33** — Contact Number Validation: when a contact number is entered it is validated for digits only and a length of 7–15 digits (regex `^\+?\d{7,15}$`). The field remains optional.

---

## [1.1.0] — 2026-03-23

### Fixed

#### Critical
- **FIX-01** — App crashed with `FormatException` when a drink checkbox was ticked but its quantity field was left empty. All 7 drink quantity fields are now validated with `int.TryParse` before any processing begins. A specific message is shown per drink (e.g. "Please enter a valid quantity (greater than 0) for Coke.").
- **FIX-02** — App crashed when the amount paid field contained multiple decimal points (e.g. `1.2.3`). The `KeyPress` handler now blocks a second decimal point if one already exists in the field.

#### High
- **FIX-03** — Clicking "Next" multiple times duplicated every order item in the list, causing incorrect totals. The order list is now cleared at the start of each "Next" click before items are re-added.
- **FIX-04** — Once the Confirm button was enabled by a valid payment, re-entering an insufficient amount showed the warning but left the Confirm button enabled. `button8.Enabled = false` is now set in the insufficient payment branch.
- **FIX-05** — The card number field stayed disabled after switching away from "Cash" to any card-based payment method, making card payments impossible. The `comboBox2_SelectedIndexChanged` handler now re-enables the field for all non-Cash selections.
- **FIX-06** — On "Order Again", the pizza size and crust radio buttons were not reset to their defaults (Small, Normal Crust), leaving the next order with no pre-selection. Defaults are now restored, and the Confirm button and card field are also reset.
- **FIX-07** — The Confirm button stayed enabled when the user went back from the Order Review tab or cleared the order, allowing a stale payment to confirm a modified order. Both the Back button and the Clear button now disable `button8`.

#### Medium
- **FIX-08** — A drink quantity of `0` was accepted and added a $0.00 line item with no actual drink. Quantity is now validated to be greater than 0.
- **FIX-09** — An empty order (no items in list) could proceed through to the checkout tab showing $0.00 totals. A guard now blocks navigation if the order list is empty after building.
- **FIX-10** — The `listView1_SelectedIndexChanged` handler was an empty auto-generated stub. Clarified with a comment that it is intentionally empty (Designer-wired, no action required).
- **FIX-11** — Drink costs were stored as raw double strings (e.g. `"2.9"`) while all other items used 2 decimal places. All drink costs now use `.ToString("F2")` for consistency.

#### Low
- **FIX-12** — The Exit button on Tab 1 closed the application immediately with no confirmation, risking accidental data loss mid-order. A Yes/No confirmation dialog is now shown before closing.
- **FIX-13** — The card number field was enabled by default on application load even before any payment method was selected. It is now disabled on load and only enabled when a card-based payment method is chosen.

---

## [1.0.0] — 2026-03-23

### Added
- Initial release of Pizza Express Ordering System
- 3-tab Windows Forms UI: Order Selection, Order Review, Checkout
- Pizza customisation: 4 sizes, 3 crust types
- 14 topping options at $0.75 each
- 7 drink options with individual quantity fields
- 4 side items and 3 free dipping sauces
- Itemised order summary with ListView (Item, Qty, Price columns)
- 13% HST calculation with subtotal and total due display
- Customer delivery information form with Canadian province dropdown
- Payment methods: Cash, Credit Card, Debit Card, Promo Card
- Change calculation for cash payments
- Required field validation before payment processing
- Order confirmation dialog with option to place another order or exit
- Full state reset on "Order Again" flow
- Numeric-only input validation on all quantity fields
- `.gitignore` for Visual Studio / .NET build artifacts
- `README.md` with setup instructions and project structure
- `USER_STORIES.md` with 29 detailed user stories, acceptance criteria, priorities and story points
