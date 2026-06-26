import React, { useEffect, useMemo, useState } from "react";
import {
    Button,
    Card,
    CardBody,
    CardHeader,
    Input,
    Option,
    Select,
    Typography,
} from "@material-tailwind/react";
import { toast } from "react-toastify";
import apiClient from "@/api/axiosConfig";

const recordTypes = [
    { value: "1", label: "Gelir" },
    { value: "2", label: "Gider" },
    { value: "3", label: "Odeme" },
    { value: "4", label: "Devir" },
    { value: "5", label: "Duzeltme" },
];

const defaultCategories = ["Tasima", "Mazot", "Aidat", "Odeme", "Yol Belgesi", "Diger"];

const emptyForm = {
    date: new Date().toISOString().slice(0, 10),
    type: "1",
    category: "Tasima",
    company: "",
    waybillNo: "",
    description: "",
    quantityKg: "",
    unitPrice: "",
    amount: "",
};

const money = (value) =>
    Number(value || 0).toLocaleString("tr-TR", {
        style: "currency",
        currency: "TRY",
        minimumFractionDigits: 2,
    });

const typeLabel = (type) => recordTypes.find((item) => item.value === String(type))?.label || "Kayit";

export function AccountingPage() {
    const [users, setUsers] = useState([]);
    const [selectedUserId, setSelectedUserId] = useState("");
    const [selectedVehicleId, setSelectedVehicleId] = useState("");
    const [records, setRecords] = useState([]);
    const [summary, setSummary] = useState(null);
    const [monthlySummaries, setMonthlySummaries] = useState([]);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [editingId, setEditingId] = useState(null);
    const [formData, setFormData] = useState(emptyForm);
    const [filters, setFilters] = useState({ startDate: "", endDate: "", category: "" });

    const selectedUser = useMemo(
        () => users.find((user) => String(user.id) === String(selectedUserId)),
        [users, selectedUserId]
    );

    const queryString = () => {
        const params = new URLSearchParams();
        if (filters.startDate) params.append("startDate", filters.startDate);
        if (filters.endDate) params.append("endDate", filters.endDate);
        if (filters.category) params.append("category", filters.category);
        const query = params.toString();
        return query ? `?${query}` : "";
    };

    const fetchUsers = async () => {
        try {
            const response = await apiClient.get("/accounting/users");
            const data = response.data || [];
            setUsers(data);
            if (data.length > 0 && !selectedUserId) {
                setSelectedUserId(String(data[0].id));
                setSelectedVehicleId(data[0].vehicles?.[0]?.id ? String(data[0].vehicles[0].id) : "");
            }
        } catch (err) {
            console.error(err);
            toast.error("Kullanici listesi yuklenemedi.");
        } finally {
            setLoading(false);
        }
    };

    const fetchAccountingData = async () => {
        if (!selectedUserId) return;

        try {
            const query = queryString();
            const [recordsRes, summaryRes, monthlyRes] = await Promise.all([
                apiClient.get(`/accounting/users/${selectedUserId}/records${query}`),
                apiClient.get(`/accounting/users/${selectedUserId}/summary${query}`),
                apiClient.get(`/accounting/users/${selectedUserId}/monthly-summary${query}`),
            ]);
            setRecords(recordsRes.data || []);
            setSummary(summaryRes.data || null);
            setMonthlySummaries(monthlyRes.data || []);
        } catch (err) {
            console.error(err);
            toast.error("Cari bilgiler yuklenemedi.");
        }
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    useEffect(() => {
        const firstVehicleId = selectedUser?.vehicles?.[0]?.id;
        setSelectedVehicleId(firstVehicleId ? String(firstVehicleId) : "");
        setEditingId(null);
        setFormData(emptyForm);
    }, [selectedUserId]);

    useEffect(() => {
        fetchAccountingData();
    }, [selectedUserId]);

    const handleChange = (name, value) => {
        setFormData((prev) => ({ ...prev, [name]: value }));
    };

    const toNullableNumber = (value) => {
        if (value === "" || value === null || value === undefined) return null;
        return Number(String(value).replace(",", "."));
    };

    const buildPayload = () => ({
        vehicleId: selectedVehicleId ? Number(selectedVehicleId) : null,
        date: formData.date,
        type: Number(formData.type),
        category: formData.category,
        company: formData.company?.trim() || null,
        waybillNo: formData.waybillNo?.trim() || null,
        description: formData.description?.trim() || null,
        quantityKg: toNullableNumber(formData.quantityKg),
        unitPrice: toNullableNumber(formData.unitPrice),
        amount: toNullableNumber(formData.amount),
    });

    const handleSubmit = async () => {
        if (!selectedUserId) {
            toast.error("Once kullanici seciniz.");
            return;
        }
        if (!formData.date || !formData.category) {
            toast.error("Tarih ve kategori zorunludur.");
            return;
        }

        setSaving(true);
        try {
            if (editingId) {
                await apiClient.put(`/accounting/records/${editingId}`, buildPayload());
                toast.success("Cari kayit guncellendi.");
            } else {
                await apiClient.post(`/accounting/users/${selectedUserId}/records`, buildPayload());
                toast.success("Cari kayit eklendi.");
            }
            setEditingId(null);
            setFormData(emptyForm);
            await fetchAccountingData();
        } catch (err) {
            console.error(err);
            toast.error(err.response?.data?.message || "Kayit islemi basarisiz oldu.");
        } finally {
            setSaving(false);
        }
    };

    const handleEdit = (record) => {
        setEditingId(record.id);
        setSelectedVehicleId(String(record.vehicleId));
        setFormData({
            date: record.date?.slice(0, 10) || emptyForm.date,
            type: String(record.type),
            category: record.category || "Diger",
            company: record.company || "",
            waybillNo: record.waybillNo || "",
            description: record.description || "",
            quantityKg: record.quantityKg ?? "",
            unitPrice: record.unitPrice ?? "",
            amount: record.quantityKg && record.unitPrice ? "" : Math.abs(record.netAmount || record.balanceEffect || 0),
        });
        window.scrollTo({ top: 0, behavior: "smooth" });
    };

    const handleDelete = async (record) => {
        if (!window.confirm(`${record.category} kaydi silinsin mi?`)) return;

        try {
            await apiClient.delete(`/accounting/records/${record.id}`);
            toast.success("Cari kayit silindi.");
            await fetchAccountingData();
        } catch (err) {
            console.error(err);
            toast.error("Kayit silinemedi.");
        }
    };

    if (loading) {
        return <Typography className="mt-12 text-center">Cari ekran yukleniyor...</Typography>;
    }

    return (
        <div className="mt-12 mb-8 flex flex-col gap-8">
            <Card>
                <CardHeader variant="gradient" color="gray" className="mb-4 p-6 flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                    <div>
                        <Typography variant="h6" color="white">Cari Hesaplar</Typography>
                        <Typography variant="small" color="white" className="opacity-70 font-normal">
                            Kullanici bazli gelir, gider ve aylik kar zarar takibi
                        </Typography>
                    </div>
                    <div className="w-full md:w-96">
                        <Select label="Kullanici Sec" value={selectedUserId} onChange={(val) => setSelectedUserId(val)}>
                            {users.map((user) => (
                                <Option key={user.id} value={String(user.id)}>
                                    {user.fullName} {user.vehicles?.[0]?.licensePlate ? `- ${user.vehicles[0].licensePlate}` : ""}
                                </Option>
                            ))}
                        </Select>
                    </div>
                </CardHeader>
                <CardBody className="grid gap-4 md:grid-cols-4">
                    <div className="rounded-lg border border-blue-gray-50 p-4">
                        <Typography variant="small" className="text-blue-gray-400 font-bold uppercase">Kullanici</Typography>
                        <Typography className="font-semibold text-blue-gray-700">{selectedUser?.fullName || "-"}</Typography>
                    </div>
                    <div className="rounded-lg border border-blue-gray-50 p-4">
                        <Typography variant="small" className="text-blue-gray-400 font-bold uppercase">Gelir</Typography>
                        <Typography className="font-semibold text-green-700">{money(summary?.incomeTotal)}</Typography>
                    </div>
                    <div className="rounded-lg border border-blue-gray-50 p-4">
                        <Typography variant="small" className="text-blue-gray-400 font-bold uppercase">Gider/Odeme</Typography>
                        <Typography className="font-semibold text-red-700">{money(summary?.outgoingTotal)}</Typography>
                    </div>
                    <div className="rounded-lg border border-blue-gray-50 p-4">
                        <Typography variant="small" className="text-blue-gray-400 font-bold uppercase">Bakiye</Typography>
                        <Typography className={`font-semibold ${(summary?.balance || 0) >= 0 ? "text-blue-gray-800" : "text-red-700"}`}>
                            {money(summary?.balance)}
                        </Typography>
                    </div>
                </CardBody>
            </Card>

            <Card>
                <CardHeader variant="gradient" color="gray" className="mb-4 p-6">
                    <Typography variant="h6" color="white">{editingId ? "Cari Kayit Duzenle" : "Yeni Cari Kayit"}</Typography>
                </CardHeader>
                <CardBody className="grid gap-4 md:grid-cols-4">
                    {selectedUser?.vehicles?.length > 1 && (
                        <Select label="Arac" value={selectedVehicleId} onChange={(val) => setSelectedVehicleId(val)}>
                            {selectedUser.vehicles.map((vehicle) => (
                                <Option key={vehicle.id} value={String(vehicle.id)}>
                                    {vehicle.licensePlate}
                                </Option>
                            ))}
                        </Select>
                    )}
                    <Input type="date" label="Tarih" value={formData.date} onChange={(e) => handleChange("date", e.target.value)} />
                    <Select label="Tip" value={formData.type} onChange={(val) => handleChange("type", val)}>
                        {recordTypes.map((type) => <Option key={type.value} value={type.value}>{type.label}</Option>)}
                    </Select>
                    <Select label="Kategori" value={formData.category} onChange={(val) => handleChange("category", val)}>
                        {defaultCategories.map((category) => <Option key={category} value={category}>{category}</Option>)}
                    </Select>
                    <Input label="Firma" value={formData.company} onChange={(e) => handleChange("company", e.target.value)} />
                    <Input label="Irsaliye No" value={formData.waybillNo} onChange={(e) => handleChange("waybillNo", e.target.value)} />
                    <Input label="Aciklama" value={formData.description} onChange={(e) => handleChange("description", e.target.value)} />
                    <Input type="number" step="0.01" label="Ton/Kg" value={formData.quantityKg} onChange={(e) => handleChange("quantityKg", e.target.value)} />
                    <Input type="number" step="0.0001" label="B. Fiyat" value={formData.unitPrice} onChange={(e) => handleChange("unitPrice", e.target.value)} />
                    <Input type="number" step="0.01" label="Tutar" value={formData.amount} onChange={(e) => handleChange("amount", e.target.value)} />
                    <div className="md:col-span-4 flex justify-end gap-3">
                        {editingId && (
                            <Button variant="text" color="blue-gray" onClick={() => { setEditingId(null); setFormData(emptyForm); }}>
                                Vazgec
                            </Button>
                        )}
                        <Button color="green" onClick={handleSubmit} disabled={saving}>
                            {saving ? "Kaydediliyor..." : editingId ? "Guncelle" : "Kaydet"}
                        </Button>
                    </div>
                </CardBody>
            </Card>

            <Card>
                <CardHeader variant="gradient" color="gray" className="mb-4 p-6 flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                    <Typography variant="h6" color="white">Aylik Ozet</Typography>
                    <div className="grid w-full gap-3 md:w-auto md:grid-cols-4">
                        <Input type="date" label="Baslangic" value={filters.startDate} onChange={(e) => setFilters({ ...filters, startDate: e.target.value })} />
                        <Input type="date" label="Bitis" value={filters.endDate} onChange={(e) => setFilters({ ...filters, endDate: e.target.value })} />
                        <Input label="Kategori" value={filters.category} onChange={(e) => setFilters({ ...filters, category: e.target.value })} />
                        <Button color="white" variant="text" onClick={fetchAccountingData}>Filtrele</Button>
                    </div>
                </CardHeader>
                <CardBody className="overflow-x-auto px-0 pt-0 pb-2">
                    <table className="w-full min-w-[780px] table-auto">
                        <thead>
                        <tr>
                            {["Ay", "Gelir", "Gider", "Odeme", "Kar/Zarar", "Bakiye", "Kayit"].map((head) => (
                                <th key={head} className="border-b border-blue-gray-50 py-3 px-5 text-left">
                                    <Typography variant="small" className="font-bold uppercase text-blue-gray-400">{head}</Typography>
                                </th>
                            ))}
                        </tr>
                        </thead>
                        <tbody>
                        {monthlySummaries.map((item) => (
                            <tr key={`${item.year}-${item.month}`} className="hover:bg-blue-gray-50/50">
                                <td className="py-3 px-5 border-b border-blue-gray-50 font-semibold">{item.monthName}</td>
                                <td className="py-3 px-5 border-b border-blue-gray-50 text-green-700">{money(item.incomeTotal)}</td>
                                <td className="py-3 px-5 border-b border-blue-gray-50 text-red-700">{money(item.expenseTotal)}</td>
                                <td className="py-3 px-5 border-b border-blue-gray-50 text-red-700">{money(item.paymentTotal)}</td>
                                <td className={`py-3 px-5 border-b border-blue-gray-50 font-semibold ${item.profitLoss >= 0 ? "text-green-700" : "text-red-700"}`}>
                                    {money(item.profitLoss)}
                                </td>
                                <td className={`py-3 px-5 border-b border-blue-gray-50 font-semibold ${item.runningBalance >= 0 ? "text-blue-gray-800" : "text-red-700"}`}>
                                    {money(item.runningBalance)}
                                </td>
                                <td className="py-3 px-5 border-b border-blue-gray-50">{item.recordCount}</td>
                            </tr>
                        ))}
                        {monthlySummaries.length === 0 && (
                            <tr><td colSpan={7} className="p-6 text-center text-blue-gray-400">Aylik ozet bulunamadi.</td></tr>
                        )}
                        </tbody>
                    </table>
                </CardBody>
            </Card>

            <Card>
                <CardHeader variant="gradient" color="gray" className="mb-4 p-6">
                    <Typography variant="h6" color="white">Cari Hareketler</Typography>
                </CardHeader>
                <CardBody className="overflow-x-auto px-0 pt-0 pb-2">
                    <table className="w-full min-w-[1040px] table-auto">
                        <thead>
                        <tr>
                            {["Tarih", "Plaka", "Tip", "Kategori", "Firma", "Irsaliye", "Aciklama", "Toplam", "Bakiye Etkisi", "Islem"].map((head) => (
                                <th key={head} className="border-b border-blue-gray-50 py-3 px-4 text-left">
                                    <Typography variant="small" className="font-bold uppercase text-blue-gray-400">{head}</Typography>
                                </th>
                            ))}
                        </tr>
                        </thead>
                        <tbody>
                        {records.map((record) => (
                            <tr key={record.id} className="hover:bg-blue-gray-50/50">
                                <td className="py-3 px-4 border-b border-blue-gray-50">{record.date?.slice(0, 10)}</td>
                                <td className="py-3 px-4 border-b border-blue-gray-50 font-semibold">{record.licensePlate}</td>
                                <td className="py-3 px-4 border-b border-blue-gray-50">{typeLabel(record.type)}</td>
                                <td className="py-3 px-4 border-b border-blue-gray-50">{record.category}</td>
                                <td className="py-3 px-4 border-b border-blue-gray-50">{record.company || "-"}</td>
                                <td className="py-3 px-4 border-b border-blue-gray-50">{record.waybillNo || "-"}</td>
                                <td className="py-3 px-4 border-b border-blue-gray-50">{record.description || "-"}</td>
                                <td className="py-3 px-4 border-b border-blue-gray-50">{money(record.grossAmount || record.netAmount)}</td>
                                <td className={`py-3 px-4 border-b border-blue-gray-50 font-semibold ${record.balanceEffect >= 0 ? "text-green-700" : "text-red-700"}`}>
                                    {money(record.balanceEffect)}
                                </td>
                                <td className="py-3 px-4 border-b border-blue-gray-50">
                                    <div className="flex gap-2">
                                        <Button size="sm" variant="text" color="blue" onClick={() => handleEdit(record)}>Duzenle</Button>
                                        <Button size="sm" variant="text" color="red" onClick={() => handleDelete(record)}>Sil</Button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                        {records.length === 0 && (
                            <tr><td colSpan={10} className="p-6 text-center text-blue-gray-400">Cari hareket bulunamadi.</td></tr>
                        )}
                        </tbody>
                    </table>
                </CardBody>
            </Card>
        </div>
    );
}

export default AccountingPage;
