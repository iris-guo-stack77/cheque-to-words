(function () {
  "use strict";

  const form = document.getElementById("cheque-form");
  if (!form) return; // not on the convert page

  const amountInput = document.getElementById("amount-input");
  const resultLine = document.getElementById("result-line");
  const feedbackArea = document.getElementById("feedback-area");
  const copyButton = document.getElementById("copy-button");

  let requestSeq = 0;

  function renderFeedback(error, wasRounded) {
    feedbackArea.innerHTML = "";
    if (error) {
      const p = document.createElement("p");
      p.id = "error-message";
      p.className = "feedback feedback-error";
      p.setAttribute("role", "alert");
      p.textContent = error;
      feedbackArea.appendChild(p);
    }
    if (wasRounded) {
      const p = document.createElement("p");
      p.id = "rounding-note";
      p.className = "feedback feedback-note";
      p.textContent = "Amount was rounded to the nearest cent.";
      feedbackArea.appendChild(p);
    }
  }

  function renderResult(words) {
    if (words) {
      resultLine.textContent = words;
      resultLine.classList.remove("is-empty");
      copyButton.disabled = false;
    } else {
      resultLine.textContent = "—"; // em dash placeholder
      resultLine.classList.add("is-empty");
      copyButton.disabled = true;
    }
  }

  async function convert(rawAmount) {
    const seq = ++requestSeq;

    if (!rawAmount || !rawAmount.trim()) {
      renderResult(null);
      renderFeedback(null, false);
      return;
    }

    let response;
    try {
      response = await fetch("/api/convert", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ amount: rawAmount })
      });
    } catch {
      if (seq !== requestSeq) return; // a newer request already landed
      renderResult(null);
      renderFeedback("Could not reach the server. Check your connection and try again.", false);
      return;
    }

    if (seq !== requestSeq) return; // a newer keystroke's request has since started

    const data = await response.json();
    if (data.success) {
      renderResult(data.words);
      renderFeedback(null, data.wasRounded);
    } else {
      renderResult(null);
      renderFeedback(data.error, false);
    }
  }

  let debounceTimer;
  amountInput.addEventListener("input", () => {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => convert(amountInput.value), 250);
  });

  form.addEventListener("submit", (event) => {
    event.preventDefault();
    clearTimeout(debounceTimer);
    convert(amountInput.value);
  });

  copyButton.addEventListener("click", async () => {
    const text = resultLine.textContent;
    if (!text || resultLine.classList.contains("is-empty")) return;

    try {
      await navigator.clipboard.writeText(text);
      const original = copyButton.textContent;
      copyButton.textContent = "Copied!";
      setTimeout(() => {
        copyButton.textContent = original;
      }, 1500);
    } catch {
      // Clipboard API unavailable (e.g. insecure context) — quietly no-op,
      // the text is still visible for the user to select and copy manually.
    }
  });

  // If the page was server-rendered with a result already (JS-disabled fallback
  // that then loaded JS on the next page), the copy button should reflect it.
  if (resultLine && !resultLine.classList.contains("is-empty") && resultLine.textContent.trim() !== "—") {
    copyButton.disabled = false;
  }
})();
