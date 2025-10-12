import { useState, useEffect } from "react";

interface VetProfile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  specialization?: string;
  licenseNumber?: string;
  yearsExperience?: number;
  bio?: string;
  isActive: boolean;
}

const VetProfilePage = () => {
  const [profile, setProfile] = useState<VetProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // TODO: Implement vet profile API
      // For now, we'll use mock data
      const mockProfile: VetProfile = {
        id: "vet-1",
        firstName: "Dr. John",
        lastName: "Smith",
        email: "john.smith@petcareplus.com",
        phoneNumber: "+1 (555) 123-4567",
        specialization: "Small Animal Medicine",
        licenseNumber: "VET12345",
        yearsExperience: 10,
        bio: "Experienced veterinarian with a passion for animal care and welfare.",
        isActive: true
      };
      
      setTimeout(() => {
        setProfile(mockProfile);
        setLoading(false);
      }, 500);
      
    } catch (error) {
      console.error("Failed to load profile:", error);
      setError("Failed to load profile. Please try again.");
      setLoading(false);
    }
  };

  const handleSave = async (updatedProfile: VetProfile) => {
    try {
      setError(null);
      setSuccess(null);
      
      // TODO: Implement save profile API
      console.log("Saving profile:", updatedProfile);
      
      // Mock save
      setTimeout(() => {
        setProfile(updatedProfile);
        setEditing(false);
        setSuccess("Profile updated successfully!");
        
        // Clear success message after 3 seconds
        setTimeout(() => setSuccess(null), 3000);
      }, 500);
      
    } catch (error) {
      console.error("Failed to save profile:", error);
      setError("Failed to save profile. Please try again.");
    }
  };

  const ProfileForm = ({ profile, onSave, onCancel }: {
    profile: VetProfile;
    onSave: (profile: VetProfile) => void;
    onCancel: () => void;
  }) => {
    const [formData, setFormData] = useState(profile);

    const handleSubmit = (e: React.FormEvent) => {
      e.preventDefault();
      onSave(formData);
    };

    return (
      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              First Name
            </label>
            <input
              type="text"
              value={formData.firstName}
              onChange={(e) => setFormData({...formData, firstName: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Last Name
            </label>
            <input
              type="text"
              value={formData.lastName}
              onChange={(e) => setFormData({...formData, lastName: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Email
          </label>
          <input
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({...formData, email: e.target.value})}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Phone Number
          </label>
          <input
            type="tel"
            value={formData.phoneNumber || ""}
            onChange={(e) => setFormData({...formData, phoneNumber: e.target.value})}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Specialization
            </label>
            <input
              type="text"
              value={formData.specialization || ""}
              onChange={(e) => setFormData({...formData, specialization: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              License Number
            </label>
            <input
              type="text"
              value={formData.licenseNumber || ""}
              onChange={(e) => setFormData({...formData, licenseNumber: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Years of Experience
          </label>
          <input
            type="number"
            value={formData.yearsExperience || ""}
            onChange={(e) => setFormData({...formData, yearsExperience: parseInt(e.target.value) || 0})}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            min="0"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Bio
          </label>
          <textarea
            value={formData.bio || ""}
            onChange={(e) => setFormData({...formData, bio: e.target.value})}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            rows={4}
            placeholder="Tell us about yourself and your expertise..."
          />
        </div>

        <div className="flex justify-end space-x-4">
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Save Changes
          </button>
        </div>
      </form>
    );
  };

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="text-lg text-slate-600 animate-pulse">
          Loading profile...
        </div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="text-center py-8 text-red-600">
        <p>Failed to load profile</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-800">
            Veterinarian Profile
          </h1>
          <p className="mt-2 text-slate-600">
            Manage your professional information and credentials
          </p>
        </div>
        {!editing && (
          <button
            onClick={() => setEditing(true)}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Edit Profile
          </button>
        )}
      </div>

      {/* Success Message */}
      {success && (
        <div className="rounded-2xl bg-green-50 p-4 text-green-800 ring-1 ring-green-200">
          <p>{success}</p>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="rounded-2xl bg-red-50 p-4 text-red-800 ring-1 ring-red-200">
          <p>{error}</p>
        </div>
      )}

      {/* Profile Content */}
      <div className="bg-white rounded-xl shadow-md p-6">
        {editing ? (
          <ProfileForm
            profile={profile}
            onSave={handleSave}
            onCancel={() => {
              setEditing(false);
              setError(null);
              setSuccess(null);
            }}
          />
        ) : (
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Name</h3>
                <p className="text-lg text-gray-900">{profile.firstName} {profile.lastName}</p>
              </div>
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Email</h3>
                <p className="text-lg text-gray-900">{profile.email}</p>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Phone Number</h3>
                <p className="text-lg text-gray-900">{profile.phoneNumber || "Not provided"}</p>
              </div>
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Specialization</h3>
                <p className="text-lg text-gray-900">{profile.specialization || "Not provided"}</p>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">License Number</h3>
                <p className="text-lg text-gray-900">{profile.licenseNumber || "Not provided"}</p>
              </div>
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Years of Experience</h3>
                <p className="text-lg text-gray-900">{profile.yearsExperience || "Not provided"}</p>
              </div>
            </div>

            <div>
              <h3 className="text-sm font-medium text-gray-500 mb-1">Bio</h3>
              <p className="text-lg text-gray-900">{profile.bio || "No bio provided"}</p>
            </div>

            <div>
              <h3 className="text-sm font-medium text-gray-500 mb-1">Status</h3>
              <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                profile.isActive 
                  ? "bg-green-100 text-green-800" 
                  : "bg-red-100 text-red-800"
              }`}>
                {profile.isActive ? "Active" : "Inactive"}
              </span>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default VetProfilePage;