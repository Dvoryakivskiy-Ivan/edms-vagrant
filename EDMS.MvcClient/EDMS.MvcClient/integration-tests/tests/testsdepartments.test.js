const axios = require("axios");

const BASE_URL = process.env.BASE_URL || "http://localhost:5112";
console.log("BASE_URL =", BASE_URL);

describe("Departments API (v1)", () => {
    test("GET /api/v1/departments returns array", async () => {
        const res = await axios.get(`${BASE_URL}/api/v1/departments`);
        expect(res.status).toBe(200);
        expect(Array.isArray(res.data)).toBe(true);
    });

    test("POST /api/v1/departments creates department", async () => {
        const payload = {
            name: "QA Dep " + Date.now(),
            code: "QA" + String(Date.now()).slice(-4),
        };

        const created = await axios.post(`${BASE_URL}/api/v1/departments`, payload);

        expect(created.status).toBe(201);
        expect(created.data).toHaveProperty("id");
        expect(created.data.name).toBe(payload.name);
        expect(created.data.code).toBe(payload.code);
    });
});
