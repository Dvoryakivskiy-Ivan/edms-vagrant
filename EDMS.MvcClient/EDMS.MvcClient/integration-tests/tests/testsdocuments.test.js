const axios = require("axios");

const BASE_URL = process.env.BASE_URL || "http://localhost:5112";
console.log("BASE_URL =", BASE_URL);
describe("Documents API (v1)", () => {
    test("GET /api/v1/documents returns array", async () => {
        const res = await axios.get(`${BASE_URL}/api/v1/documents`);
        expect(res.status).toBe(200);
        expect(Array.isArray(res.data)).toBe(true);
    });

    test("GET /api/v1/documents/{id} returns 404 for missing", async () => {
        try {
            await axios.get(`${BASE_URL}/api/v1/documents/9999999`);
            throw new Error("Expected 404 but request succeeded");
        } catch (err) {
            expect(err.response.status).toBe(404);
        }
    });
});
