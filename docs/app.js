"use strict";

const storageKey = "graveyard-orbit.callsign";
const form = document.querySelector("#callsign-form");
const input = document.querySelector("#callsign");
const greeting = document.querySelector("#greeting");

function showCallsign(value) {
  // textContent, never innerHTML: callsigns are player input, not trusted markup.
  greeting.textContent = value ? `Welcome aboard, ${value}. Keep your orbit clean.` : "Welcome aboard, pilot.";
}

try {
  const saved = (localStorage.getItem(storageKey) || "").slice(0, 24);
  input.value = saved;
  showCallsign(saved);
} catch {
  // Private browsing or restricted storage must not break the rest of the page.
}

form.addEventListener("submit", (event) => {
  event.preventDefault();
  const value = input.value.trim().slice(0, 24);
  if (!value) {
    input.setCustomValidity("Enter a callsign, not just spaces.");
    input.reportValidity();
    return;
  }
  input.setCustomValidity("");
  input.value = value;
  showCallsign(value);
  try {
    localStorage.setItem(storageKey, value);
  } catch {
    greeting.textContent += " Browser storage is unavailable; this callsign lasts until reload.";
  }
});
input.addEventListener("input", () => input.setCustomValidity(""));
