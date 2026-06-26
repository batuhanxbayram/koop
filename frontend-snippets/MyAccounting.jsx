import React, { useEffect, useState } from "react";
import { Button, Card, CardBody, CardHeader, Input, Typography } from "@material-tailwind/react";
import { toast } from "react-toastify";
import apiClient from "@/api/axiosConfig";

const money = (value) =>
    Number(value || 0).toLocaleString("tr-TR", {
        style: "currency",
        currency: "TRY",
        minimumFractionDigits: 2,
    });

const typeLabels = {
    1: "Gelir",
    2: "Gider",
    3: "Odeme",
    4: "Devir",
    5: "Duzeltme",
};

export function MyAccounting() {
    const [records, setRecords] = useState([]);
    const [summary, setSummary] = useState(null);
    const [monthlySummaries, setMonthlySummaries] = useState([]);
    const [loading, setLoading] = useState(true);
    const [filters, setFilters] = useState({ startDate: "", endDate: "", category: "" });

    const queryString = () => {
        const params = new URLSearchParams();
        if (filters.startDate) params.append("startDate", filters.startDate);
        if (filters.endDate) params.append("endDate", filters.endDate);
        if (filters.category) params.append("category", filters.category);
        const query = params.toString();
        return query ? `?${query}` : "";
    };

    const fetchData = async () => {
        try {
            const query = queryString();
            const [recordsRes, summaryRes, monthlyRes] = await Promise.all([
                apiClient.get(`/accounting/my-records${query}`),
                apiClient.get(`/accounting/my-user-summary${query}`),
                apiClient.get(`/accounting/my-monthly-summary${query}`),
            ]);
            setRecords(recordsRes.data || []);
            setSummary(summaryRes.data || null);
            setMonthlySummaries(monthlyRes.data || []);
        } catch (err) {
            console.error(err);
            toast.error("Cari bilgileriniz yuklenemedi.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, []);

    if (loading) {
        return <Typography className="mt-12 text-center">Cari bilgiler yukleniyor...</Typography>;
    }

    return (
        <div className="mt-12 mb-8 flex flex-col gap-8">
            <Card>
                <CardHeader variant="gradient" color="gray" className="mb-4 p-6">
                    <Typography variant="h6" color="white">Cari Ozetim</Typography>
                    <Typography variant="small" color="white" className="opacity-70 font-normal">
                        Aylik gelir, gider ve bakiye durumunuz
                    </Typography>
                </CardHeader>
                <CardBody className="grid gap-4 md:grid-cols-4">
                    <div className="rounded-lg border border-blue-gray-50 p-4">
                        <Typography variant="small" className="text-blue-gray-400 font-bold uppercase">Toplam Gelir</Typography>
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
                    <div className="rounded-lg border border-blue-gray-50 p-4">
                        <Typography variant="small" className="text-blue-gray-400 font-bold uppercase">Kayit</Typography>
                        <Typography className="font-semibold text-blue-gray-700">{summary?.recordCount || 0}</Typography>
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
                        <Button color="white" variant="text" onClick={fetchData}>Filtrele</Button>
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
                    <Typography variant="h6" color="white">Cari Hareketlerim</Typography>
                </CardHeader>
                <CardBody className="overflow-x-auto px-0 pt-0 pb-2">
                    <table className="w-full min-w-[900px] table-auto">
                        <thead>
                        <tr>
                            {["Tarih", "Plaka", "Tip", "Kategori", "Firma", "Aciklama", "Toplam", "Bakiye Etkisi"].map((head) => (
                                <th key={head} className="border-b border-blue-gray-50 py-3 px-5 text-left">
                                    <Typography variant="small" className="font-bold uppercase text-blue-gray-400">{head}</Typography>
                                </th>
                            ))}
                        </tr>
                        </thead>
                        <tbody>
                        {records.map((record) => (
                            <tr key={record.id} className="hover:bg-blue-gray-50/50">
                                <td className="py-3 px-5 border-b border-blue-gray-50">{record.date?.slice(0, 10)}</td>
                                <td className="py-3 px-5 border-b border-blue-gray-50 font-semibold">{record.licensePlate}</td>
                                <td className="py-3 px-5 border-b border-blue-gray-50">{typeLabels[record.type] || record.typeName}</td>
                                <td className="py-3 px-5 border-b border-blue-gray-50">{record.category}</td>
                                <td className="py-3 px-5 border-b border-blue-gray-50">{record.company || "-"}</td>
                                <td className="py-3 px-5 border-b border-blue-gray-50">{record.description || "-"}</td>
                                <td className="py-3 px-5 border-b border-blue-gray-50">{money(record.grossAmount || record.netAmount)}</td>
                                <td className={`py-3 px-5 border-b border-blue-gray-50 font-semibold ${record.balanceEffect >= 0 ? "text-green-700" : "text-red-700"}`}>
                                    {money(record.balanceEffect)}
                                </td>
                            </tr>
                        ))}
                        {records.length === 0 && (
                            <tr><td colSpan={8} className="p-6 text-center text-blue-gray-400">Cari hareket bulunamadi.</td></tr>
                        )}
                        </tbody>
                    </table>
                </CardBody>
            </Card>
        </div>
    );
}

export default MyAccounting;
