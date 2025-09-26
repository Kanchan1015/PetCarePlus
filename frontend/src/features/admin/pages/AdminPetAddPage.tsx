import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PetForm } from '../../shared/components';
import { adminPetsApi } from '../../shared/api/petsApi';
import Breadcrumbs from '../../../components/Breadcrumbs';
import type { CreatePetRequest, UpdatePetRequest, UserSelection } from '../../shared/types/pet';

export const AdminPetAddPage: React.FC = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [users, setUsers] = useState<UserSelection[]>([]);
  const [selectedUserId, setSelectedUserId] = useState<string>('');
  const [loadingUsers, setLoadingUsers] = useState(true);

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      setLoadingUsers(true);
      const usersData = await adminPetsApi.getUsersForSelection();
      setUsers(usersData);
      if (usersData.length > 0) {
        setSelectedUserId(usersData[0].id);
      }
    } catch (error) {
      console.error('Failed to load users:', error);
      alert('Failed to load users. Please try again.');
    } finally {
      setLoadingUsers(false);
    }
  };

  const handleSubmit = async (data: CreatePetRequest | UpdatePetRequest) => {
    if (!selectedUserId) {
      alert('Please select a user for the pet.');
      return;
    }

    setIsLoading(true);
    try {
      const createData = {
        ...(data as CreatePetRequest),
        ownerUserId: selectedUserId
      };
      await adminPetsApi.create(createData);
      alert('Pet created successfully!');
      navigate('/admin/pets');
    } catch (error) {
      console.error('Failed to create pet:', error);
      alert('Failed to create pet. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancel = () => {
    navigate('/admin/pets');
  };

  const breadcrumbItems = [
    { label: 'Dashboard', to: '/admin' },
    { label: 'Pet Management', to: '/admin/pets' },
    { label: 'Add Pet' }
  ];

  if (loadingUsers) {
    return (
      <div className="space-y-6">
        <Breadcrumbs items={breadcrumbItems} />
        <div className="flex items-center justify-center h-64">
          <div className="text-lg text-gray-600">Loading users...</div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <Breadcrumbs items={breadcrumbItems} />
      
      <div className="bg-white rounded-lg shadow-sm border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-200">
          <h1 className="text-2xl font-bold text-gray-900">Add New Pet</h1>
          <p className="text-gray-600 mt-1">Fill in the details below to create a new pet record</p>
        </div>
        
        <div className="p-6">
          <div className="max-w-2xl">
            <div className="space-y-4">
              {/* User Selection */}
              <div>
                <label htmlFor="owner" className="block text-sm font-medium text-gray-700 mb-1">
                  Select Owner *
                </label>
                <select
                  id="owner"
                  value={selectedUserId}
                  onChange={(e) => setSelectedUserId(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 mb-6"
                >
                  <option value="">Select a user</option>
                  {users.map((user) => (
                    <option key={user.id} value={user.id}>
                      {user.fullName} ({user.email})
                    </option>
                  ))}
                </select>
              </div>

              {/* Pet Form */}
              <PetForm
                mode="create"
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                isLoading={isLoading}
                ownerUserId={selectedUserId}
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};