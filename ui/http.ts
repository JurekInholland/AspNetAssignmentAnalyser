export default {
  async get(resource: string) {
    try {
      const response = await fetch(`/api/${resource}`, {
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
      });

      if (!response.ok) {
        throw new Error(response.statusText);
      }

      return await response.json();
    } catch (error) {
      console.error(error);
      throw error;
    }
  },

  async post(resource: string, data: any) {
    const response = await fetch(`/api/${resource}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });

    return await response.json();
  },

  async put(resource: string, data: any) {
    const response = await fetch(`/api/${resource}`, {
      method: "PUT",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });

    return await response.json();
  },

  async delete(resource: string) {
    const response = await fetch(`/api/${resource}`, {
      method: "DELETE",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
    });

    return await response.json();
  },
};
