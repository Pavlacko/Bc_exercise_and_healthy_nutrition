document.addEventListener("DOMContentLoaded", function () {
    const weightInput = document.getElementById("bmiWeight");
    const heightInput = document.getElementById("bmiHeight");
    const button = document.getElementById("bmiCalculateBtn");
    const resultDiv = document.getElementById("bmiResult");

    button.addEventListener("click", function () {
        const weight = parseFloat(weightInput.value);
        const height = parseFloat(heightInput.value);

        if (isNaN(weight) || isNaN(height)) {
            resultDiv.textContent = "Zadajte prosím hmotnosť a výšku.";
            resultDiv.className = "fw-bold text-danger";
            return;
        }

        if (weight < 20 || weight > 300 || height < 100 || height > 250) {
            resultDiv.textContent = "Zadané hodnoty sú mimo rozsahu.";
            resultDiv.className = "fw-bold text-danger";
            return;
        }

        const heightMeters = height / 100.0;
        const bmi = weight / (heightMeters * heightMeters);
        let category = "";

        if (bmi < 18.5) {
            category = "Podváha";
        } else if (bmi < 25) {
            category = "Normálna hmotnosť";
        } else if (bmi < 30) {
            category = "Nadváha";
        } else {
            category = "Obezita";
        }

        resultDiv.textContent = `Vaše BMI je ${bmi.toFixed(1)} – ${category}.`;
        resultDiv.className = "fw-bold text-success";
    });
});