param (
    [string]$DesignerPath = "WindowsFormsApplication3\Form1.Designer.cs",
    [string]$FormPath     = "WindowsFormsApplication3\Form1.cs"
)

# ── Rename map ────────────────────────────────────────────────────────────────
# Order matters: longer / higher-numbered names FIRST to prevent prefix matches
$renames = [ordered]@{
    # Buttons — event handler method names first (contain underscores so \b won't catch them)
    "button1_Click_1"  = "btnConfirmOrder_Click"
    "button2_Click"    = "btnOrderAgain_Click"
    "button3_Click"    = "btnCheckOut_Click"
    "button4_Click"    = "btnClearOrder_Click"
    "button5_Click"    = "btnExit_Click"
    "button6_Click"    = "btnGoBack_Click"
    "button7_Click"    = "btnPay_Click"
    "button8_Click"    = "btnSubmitOrder_Click"
    "button9_Click"    = "btnAddPizzaToCart_Click"

    # TextBox event handler methods
    "textBox20_KeyPress" = "txtAmountPaid_KeyPress"
    "textBox1_KeyPress"  = "txtQtyCoke_KeyPress"
    "textBox2_KeyPress"  = "txtQtyDietCoke_KeyPress"
    "textBox3_KeyPress"  = "txtQtyIcedTea_KeyPress"
    "textBox4_KeyPress"  = "txtQtyGingerAle_KeyPress"
    "textBox5_KeyPress"  = "txtQtySprite_KeyPress"
    "textBox6_KeyPress"  = "txtQtyRootBeer_KeyPress"
    "textBox7_KeyPress"  = "txtQtyWater_KeyPress"

    "listView1_SelectedIndexChanged" = "lvOrder_SelectedIndexChanged"

    # --- Variable names (word-boundary safe) ---
    # CheckBoxes — higher numbers first to avoid prefix match
    "checkBox28" = "cbSourCreamDip"
    "checkBox27" = "cbBBQDip"
    "checkBox26" = "cbGarlicDip"
    "checkBox25" = "cbCheesyGarlicBread"
    "checkBox24" = "cbOnionRings"
    "checkBox23" = "cbPoutine"
    "checkBox22" = "cbChickenWings"
    "checkBox21" = "cbWater"
    "checkBox20" = "cbRootBeer"
    "checkBox19" = "cbSprite"
    "checkBox18" = "cbGingerAle"
    "checkBox17" = "cbIcedTea"
    "checkBox16" = "cbDietCoke"
    "checkBox15" = "cbCoke"
    "checkBox14" = "cbShreddedChicken"
    "checkBox13" = "cbRoastedGarlic"
    "checkBox12" = "cbSpinach"
    "checkBox11" = "cbSunDriedTomatoes"
    "checkBox10" = "cbAnchovies"
    "checkBox9"  = "cbDriedShrimps"
    "checkBox8"  = "cbPineapple"
    "checkBox7"  = "cbJalapeno"
    "checkBox6"  = "cbGroundBeef"
    "checkBox5"  = "cbBacon"
    "checkBox4"  = "cbHam"
    "checkBox3"  = "cbMushroom"
    "checkBox2"  = "cbExtraCheese"
    "checkBox1"  = "cbPepperoni"

    # TextBoxes — higher numbers first
    "textBox21" = "txtChange"
    "textBox20" = "txtAmountPaid"
    "textBox19" = "txtAmountDue"
    "textBox18" = "txtCardOrPromo"
    "textBox17" = "txtEmail"
    "textBox16" = "txtContactNo"
    "textBox15" = "txtPostalCode"
    "textBox14" = "txtCity"
    "textBox13" = "txtAddress"
    "textBox12" = "txtLastName"
    "textBox11" = "txtFirstName"
    "textBox10" = "txtTotalDue"
    "textBox9"  = "txtTax"
    "textBox8"  = "txtSubtotal"
    "textBox7"  = "txtQtyWater"
    "textBox6"  = "txtQtyRootBeer"
    "textBox5"  = "txtQtySprite"
    "textBox4"  = "txtQtyGingerAle"
    "textBox3"  = "txtQtyIcedTea"
    "textBox2"  = "txtQtyDietCoke"
    "textBox1"  = "txtQtyCoke"

    # RadioButtons
    "radioButton7" = "rbCrustSausage"
    "radioButton6" = "rbCrustCheesy"
    "radioButton5" = "rbCrustNormal"
    "radioButton4" = "rbSizeExtraLarge"
    "radioButton3" = "rbSizeLarge"
    "radioButton2" = "rbSizeMedium"
    "radioButton1" = "rbSizeSmall"

    # Buttons (variable names)
    "button9"  = "btnAddPizzaToCart"
    "button8"  = "btnSubmitOrder"
    "button7"  = "btnPay"
    "button6"  = "btnGoBack"
    "button5"  = "btnExit"
    "button4"  = "btnClearOrder"
    "button3"  = "btnCheckOut"
    "button2"  = "btnOrderAgain"
    "button1"  = "btnConfirmOrder"

    # ComboBoxes
    "comboBox2" = "cboPaymentMethod"
    "comboBox1" = "cboRegion"

    # NumericUpDown
    "numericUpDown1" = "nudPizzaQty"

    # ListView
    "listView1" = "lvOrder"

    # Key label (referenced in Form1.cs logic)
    "label15"   = "lblCardOrPromo"

    # Column headers
    "columnHeader3" = "colPrice"
    "columnHeader2" = "colQuantity"
    "columnHeader1" = "colItem"

    # GroupBoxes
    "groupBox7" = "grpPayment"
    "groupBox6" = "grpCustomer"
    "groupBox5" = "grpCrust"
    "groupBox4" = "grpSides"
    "groupBox3" = "grpDrinks"
    "groupBox2" = "grpToppings"
    "groupBox1" = "grpPizzaSize"
}

function Rename-File($path) {
    $content = Get-Content $path -Raw -Encoding UTF8
    foreach ($kvp in $renames.GetEnumerator()) {
        # Use word-boundary regex so "button1" doesn't match inside "button10"
        $content = [regex]::Replace($content, "(?<![A-Za-z0-9_])$([regex]::Escape($kvp.Key))(?![A-Za-z0-9_])", $kvp.Value)
    }
    [System.IO.File]::WriteAllText((Resolve-Path $path), $content, [System.Text.Encoding]::UTF8)
    Write-Host "Renamed: $path"
}

Rename-File $DesignerPath
Rename-File $FormPath

Write-Host "Done. All controls renamed."
