import React, { useEffect, useState, useCallback } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import axios from 'axios';

const App = () => {
    const [rows, setRows] = useState([]);
    // No need to manage columns state unless they change dynamically
    const columns = [
        { field: 'id', headerName: 'ID', width: 100, filterable: true, sortable: true }, // Ensure sortable is true (default)
        { field: 'name', headerName: 'Name', width: 150, filterable: true, sortable: true },
        { field: 'email', headerName: 'Email', width: 200, filterable: true, sortable: true },
    ];
    const [paginationModel, setPaginationModel] = useState({
        page: 0,
        pageSize: 10,
    });
    const [totalRecords, setTotalRecords] = useState(0);
    const [loading, setLoading] = useState(false);
    const [filterModel, setFilterModel] = useState({ items: [] });
    const [sortModel, setSortModel] = useState([]); // State for sorting [{ field: 'name', sort: 'asc' }]

    // Use useCallback to memoize fetchData and prevent unnecessary re-renders/fetches
    const fetchData = useCallback(async () => {
        setLoading(true);
        // Get sorting parameters from sortModel
        const currentSortField = sortModel.length > 0 ? sortModel[0].field : null;
        const currentSortDirection = sortModel.length > 0 ? sortModel[0].sort : null;

        try {
            const params = {
                page: paginationModel.page + 1, // API likely expects 1-based page index
                pageSize: paginationModel.pageSize,
                filters: JSON.stringify(filterModel.items), // Send only items array
            };

            // Add sort parameters if they exist
            if (currentSortField) {
                params.sortField = currentSortField;
            }
            if (currentSortDirection) {
                params.sortDirection = currentSortDirection;
            }

            const response = await axios.get('https://localhost:7081/api/Records/GetPaginatedData', {
                params: params // Pass parameters object
            });

            setRows(response.data.data);
            setTotalRecords(response.data.totalCount);
        } catch (error) {
            console.error('Error fetching data:', error);
            // Handle error state if needed (e.g., show error message)
            setRows([]); // Clear rows on error
            setTotalRecords(0);
        } finally {
            setLoading(false);
        }
    }, [paginationModel, filterModel, sortModel]); // Dependencies for useCallback

    // useEffect to trigger fetchData when dependencies change
    useEffect(() => {
        fetchData();
    }, [fetchData]); // fetchData is now stable due to useCallback

    // Handler for filter changes
    const handleFilterModelChange = (newFilterModel) => {
        // When filters change, reset to the first page
        setFilterModel(newFilterModel);
        setPaginationModel((prev) => ({ ...prev, page: 0 }));
    };

    // Handler for sort changes
    const handleSortModelChange = (newSortModel) => {
        // MUI DataGrid usually sends an array for sortModel, potentially for multi-sort
        // We'll handle single sort for this example
        setSortModel(newSortModel);
        // Optional: Reset to first page when sorting changes, depends on desired UX
        // setPaginationModel((prev) => ({ ...prev, page: 0 }));
    };

    // Handler for pagination changes
    const handlePaginationModelChange = (newPaginationModel) => {
        setPaginationModel(newPaginationModel);
    };


    return (
        <div style={{ margin: "5% auto", height: 500, width: '80%' }}> {/* Adjusted style */}
            {/* Removed <center> - prefer CSS for centering */}
            <DataGrid
                rows={rows}
                columns={columns}
                rowCount={totalRecords} // Total number of records for server-side pagination
                loading={loading} // Show loading overlay

                // Pagination
                paginationMode="server"
                paginationModel={paginationModel}
                onPaginationModelChange={handlePaginationModelChange}
                pageSizeOptions={[10, 25, 50, 100]} // Options for page size dropdown

                // Filtering
                filterMode="server"
                filterModel={filterModel}
                onFilterModelChange={handleFilterModelChange}

                // Sorting
                sortingMode="server" // <-- Enable server-side sorting
                sortModel={sortModel} // <-- Control sort model state
                onSortModelChange={handleSortModelChange} // <-- Handle sort changes

                // Other props
                disableSelectionOnClick // Deprecated, use disableRowSelectionOnClick
            // disableRowSelectionOnClick
            // autoHeight // Adjusts height to content, might remove need for fixed height style
            />
        </div>
    );
};

export default App;