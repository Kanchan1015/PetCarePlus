// OwnerRequestAppointmentPage.tsx
// Purpose: Form for owners to request new appointments for their pets
// Features: Pet selection, date/time picker with validation, reason dropdown, form submission with success messaging
// Route: /owner/appointments/request - "As an owner, I want to request appointments for my pets"

import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { appointmentsApi } from "../../shared/api/appointmentsApi";
import { petsApi } from "../../shared/api/petsApi";
import type { Pet } from "../../shared/types/pet";

export const OwnerRequestAppointmentPage: React.FC = () => {
  const navigate = useNavigate();
  const [pets, setPets] = useState<Pet[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingPets, setIsLoadingPets] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    petId: "",
    requestedDateTime: "",
    reasonForVisit: "",
    notes: "",
  });

  useEffect(() => {
    loadMyPets();
  }, []);

  const loadMyPets = async () => {
    try {
      setIsLoadingPets(true);
      const data = await petsApi.getMy();
      setPets(data.filter((pet) => pet.isActive));
    } catch (error) {
      console.error("Failed to load pets:", error);
      setError("Failed to load your pets. Please try again.");
    } finally {
      setIsLoadingPets(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      if (
        !formData.petId ||
        !formData.requestedDateTime ||
        !formData.reasonForVisit
      ) {
        setError("Please fill in all required fields.");
        return;
      }

      const requestedDate = new Date(formData.requestedDateTime);
      const now = new Date();
      if (requestedDate <= now) {
        setError("Please select a future date and time.");
        return;
      }

      await appointmentsApi.create(formData);
      setSuccess(
        "Appointment request submitted successfully! You will be notified when it's reviewed."
      );

      setFormData({
        petId: "",
        requestedDateTime: "",
        reasonForVisit: "",
        notes: "",
      });

      setTimeout(() => navigate("/owner"), 2000);
    } catch (error: any) {
      console.error("Failed to submit appointment request:", error);
      setError(
        error.response?.data?.message ||
          "Failed to submit appointment request. Please try again."
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement
    >
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const getMinDateTime = () => {
    const now = new Date();
    now.setHours(now.getHours() + 1);
    return now.toISOString().slice(0, 16);
  };

  if (isLoadingPets) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="text-lg text-slate-600 animate-pulse">
          Loading your pets...
        </div>
      </div>
    );
  }

  if (pets.length === 0) {
    return (
      <div className="rounded-3xl bg-white p-8 text-center ring-1 ring-slate-200 shadow-md">
        <div className="mb-3 text-6xl text-slate-400 animate-pulse">🐾</div>
        <h2 className="text-2xl font-semibold text-slate-800 mb-2">
          No Active Pets
        </h2>
        <p className="text-slate-600 mb-4">
          You need to have at least one active pet to request an appointment.
        </p>
        <button
          onClick={() => navigate("/owner")}
          className="inline-flex items-center rounded-2xl bg-green-600 px-4 py-2 text-sm font-semibold text-white hover:bg-green-700 focus-visible:ring focus-visible:ring-green-500 transition-colors transform hover:scale-105"
        >
          Back to Dashboard
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight text-slate-800">
          Request Appointment
        </h1>
        <p className="mt-2 text-slate-600">Schedule a visit for your pet</p>
      </div>

      {/* Success Message */}
      {success && (
        <div className="rounded-2xl bg-green-50 p-4 text-green-800 ring-1 ring-green-200 animate-fadeIn transition-opacity">
          <div className="flex items-center">
            <span className="text-xl mr-2">✅</span>
            <p>{success}</p>
          </div>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="rounded-2xl bg-red-50 p-4 text-red-800 ring-1 ring-red-200 animate-fadeIn transition-opacity">
          <div className="flex items-center">
            <span className="text-xl mr-2">❌</span>
            <p>{error}</p>
          </div>
        </div>
      )}

      {/* Form */}
      <form
        onSubmit={handleSubmit}
        className="bg-white rounded-3xl shadow-md p-6 ring-1 ring-slate-200 transition-shadow hover:shadow-lg"
      >
        <div className="space-y-6">
          {/* Pet Selection */}
          <div>
            <label
              htmlFor="petId"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Select Pet *
            </label>
            <select
              id="petId"
              name="petId"
              value={formData.petId}
              onChange={handleChange}
              required
              className="w-full rounded-xl border-0 px-3 py-2 ring-1 ring-slate-200 text-slate-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 transition-colors"
            >
              <option value="">Choose a pet...</option>
              {pets.map((pet) => (
                <option key={pet.id} value={pet.id}>
                  {pet.name} ({pet.species})
                </option>
              ))}
            </select>
          </div>

          {/* Date & Time */}
          <div>
            <label
              htmlFor="requestedDateTime"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Preferred Date & Time *
            </label>
            <input
              type="datetime-local"
              id="requestedDateTime"
              name="requestedDateTime"
              value={formData.requestedDateTime}
              onChange={handleChange}
              min={getMinDateTime()}
              required
              className="w-full rounded-xl border-0 px-3 py-2 ring-1 ring-slate-200 text-slate-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 transition-colors"
            />
            <p className="mt-1 text-xs text-gray-500">
              Please select a date and time at least 1 hour in advance
            </p>
          </div>

          {/* Reason for Visit */}
          <div>
            <label
              htmlFor="reasonForVisit"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Reason for Visit *
            </label>
            <select
              id="reasonForVisit"
              name="reasonForVisit"
              value={formData.reasonForVisit}
              onChange={handleChange}
              required
              className="w-full rounded-xl border-0 px-3 py-2 ring-1 ring-slate-200 text-slate-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 transition-colors"
            >
              <option value="">Select reason...</option>
              <option value="Routine Checkup">Routine Checkup</option>
              <option value="Vaccination">Vaccination</option>
              <option value="Illness/Symptoms">Illness/Symptoms</option>
              <option value="Injury">Injury</option>
              <option value="Dental Care">Dental Care</option>
              <option value="Surgery Consultation">Surgery Consultation</option>
              <option value="Follow-up Visit">Follow-up Visit</option>
              <option value="Emergency">Emergency</option>
              <option value="Other">Other</option>
            </select>
          </div>

          {/* Additional Notes */}
          <div>
            <label
              htmlFor="notes"
              className="block text-sm font-medium text-gray-700 mb-2"
            >
              Additional Notes
            </label>
            <textarea
              id="notes"
              name="notes"
              value={formData.notes}
              onChange={handleChange}
              rows={4}
              placeholder="Any additional information about your pet's condition or special requirements..."
              className="w-full rounded-xl border-0 px-3 py-2 ring-1 ring-slate-200 text-slate-800 placeholder:text-slate-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 transition-colors"
            />
          </div>

          {/* Buttons */}
          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={() => navigate("/owner")}
              className="inline-flex items-center rounded-2xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 focus-visible:ring focus-visible:ring-indigo-500 transition-colors transform hover:scale-105"
              disabled={isLoading}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isLoading}
              className="inline-flex items-center rounded-2xl bg-green-600 px-6 py-2 text-sm font-semibold text-white hover:bg-green-700 focus-visible:ring focus-visible:ring-green-500 transition-colors transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? (
                <>
                  <svg
                    className="animate-spin -ml-1 mr-3 h-4 w-4 text-white"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                  >
                    <circle
                      className="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      strokeWidth="4"
                    ></circle>
                    <path
                      className="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    ></path>
                  </svg>
                  Submitting...
                </>
              ) : (
                "Submit Request"
              )}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
};
