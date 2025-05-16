document.addEventListener('DOMContentLoaded', function () {
    let addTaskModal = null;
    let addTaskForm = null;
    let saveTaskButton = null;
    let addTaskFormError = null;

    let editTaskModal = null;
    let editTaskForm = null;
    let updateTaskButton = null;
    let editTaskFormError = null;
    let editingTaskId = null;


    if (IS_ADMIN) {
        const addTaskModalElement = document.getElementById('addTaskModal');
        if (addTaskModalElement) {
            if (typeof bootstrap !== 'undefined' && typeof bootstrap.Modal !== 'undefined') {
                try {
                    addTaskModal = new bootstrap.Modal(addTaskModalElement);
                } catch (e) {
                    console.error('Bootstrap modal initialization error for addTaskModal:', e);
                }
            } else {
                console.error('Bootstrap library or bootstrap.Modal is not defined.');
            }

            addTaskForm = document.getElementById('addTaskForm');
            saveTaskButton = document.getElementById('saveTaskButton');
            addTaskFormError = document.getElementById('addTaskFormError');
        }

        const editTaskModalElement = document.getElementById('editTaskModal');
        if (editTaskModalElement) {
            if (typeof bootstrap !== 'undefined' && typeof bootstrap.Modal !== 'undefined') {
                try {
                    editTaskModal = new bootstrap.Modal(editTaskModalElement);
                } catch (e) {
                    console.error('Bootstrap modal initialization error for editTaskModal:', e);
                }
            } else {
                console.error('Bootstrap library or bootstrap.Modal is not defined.');
            }
            editTaskForm = document.getElementById('editTaskForm');
            updateTaskButton = document.getElementById('updateTaskButton');
            editTaskFormError = document.getElementById('editTaskFormError');
        }
    }

    function createTaskCardHtml(taskData) {
        let adminButtons = '';
        if (IS_ADMIN) {
            adminButtons = `
                <div class="task-actions">
                    <button class="btn btn-sm btn-outline-secondary edit-task-btn py-0 px-1 me-1" title="Edytuj" data-task-id="${taskData.id}">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger delete-task-btn py-0 px-1" title="Usuń" data-task-id="${taskData.id}">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>`;
        }
        return `
            <div class="card task-card mb-2 shadow-sm" id="task-${taskData.id}" data-task-id="${taskData.id}" draggable="true">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start">
                        <h5 class="card-title mb-1">${taskData.title}</h5>
                        ${adminButtons}
                    </div>
                    ${taskData.description ? `<p class="card-text small">${taskData.description}</p>` : ''}
                    <p class="card-text mb-0">
                        <small class="text-muted">Przypisane do: ${taskData.assignedUserName || 'N/A'}</small>
                    </p>
                    <p class="card-text">
                        <small class="text-muted">Zaktualizowano: ${new Date(taskData.updatedAt).toLocaleString()}</small>
                    </p>
                </div>
            </div>`;
    }
    function updateTaskCount(columnId, change) {
        const countElement = document.getElementById(columnId + '-count');
        if (countElement) {
            let currentCount = parseInt(countElement.textContent) || 0;
            countElement.textContent = currentCount + change;
        }
    }

    //CREATE TASK
    if (IS_ADMIN && saveTaskButton && addTaskForm && addTaskModal) {
        saveTaskButton.addEventListener('click', async function () {
            if (!addTaskForm.checkValidity()) {
                addTaskForm.classList.add('was-validated');
                if (addTaskFormError) addTaskFormError.textContent = 'Please complete all required fields.';
                return;
            }
            addTaskForm.classList.remove('was-validated');
            if (addTaskFormError) addTaskFormError.textContent = '';

            const formData = new FormData(addTaskForm);

            let assignedUserIdFromForm = formData.get('AssignedUserId');
            if (assignedUserIdFromForm === "") {
                assignedUserIdFromForm = null; 
            }

            const taskData = {
                title: formData.get('Title'),
                description: formData.get('Description'),
                assignedUserId: assignedUserIdFromForm
            };

            try {
                const response = await fetch('/Tasks/Create', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': CSRF_TOKEN
                    },
                    body: JSON.stringify(taskData)
                });

                const result = await response.json();

                if (result.success && result.task) {
                    const newTaskCardHtml = createTaskCardHtml(result.task);
                    const todoColumn = document.getElementById('todo-tasks');
                    if (todoColumn) {
                        const dropIndicator = todoColumn.querySelector('.drop-indicator');
                        if (dropIndicator) {
                            todoColumn.insertBefore(document.createRange().createContextualFragment(newTaskCardHtml), dropIndicator);
                        } else {
                            todoColumn.insertAdjacentHTML('beforeend', newTaskCardHtml);
                        }
                    }
                    updateTaskCount('todo', 1);

                    addTaskModal.hide();
                    addTaskForm.reset();
                    addTaskForm.classList.remove('was-validated');
                    if (addTaskFormError) addTaskFormError.textContent = '';
                } else {
                    if (addTaskFormError) {
                        addTaskFormError.textContent = result.errors ? result.errors.join('; ') : (result.message || 'Something went wrong.');
                    }
                    console.error('Error creating task:', result);
                }
            } catch (error) {
                if (addTaskFormError) {
                    addTaskFormError.textContent = 'A network or server error occurred.';
                }
                console.error('Network error:', error);
            }
        });
    }
    //DELETE TASK
    document.body.addEventListener('click', async function (event) {
        const deleteButton = event.target.closest('.delete-task-btn');

        if (deleteButton && IS_ADMIN) {
            event.preventDefault();

            const taskIdToDelete = deleteButton.dataset.taskId;

            const taskCard = document.getElementById(`task-${taskIdToDelete}`);

            if (!taskIdToDelete || taskIdToDelete.trim() === "" || !taskCard) {
                console.error('Could not find a valid task ID or task card to delete. taskIdToDelete:', taskIdToDelete);
                return;
            }

            if (window.confirm('Are you sure you want to delete this task?')) {
                try {
                    const response = await fetch(`/Tasks/Delete/${taskIdToDelete}`, {
                        method: 'DELETE',
                        headers: {
                            'RequestVerificationToken': CSRF_TOKEN
                        }
                    });

                    if (!response.ok) {
                        let errorText = `Server error: ${response.status} ${response.statusText}`;
                        try {
                            const errorResult = await response.json();
                            errorText = errorResult.message || (errorResult.errors ? errorResult.errors.join('; ') : errorText);
                        } catch (e) { }
                        console.error('Error deleting task:', errorText);
                        alert(`Failed to delete task: ${errorText}`);
                        return;
                    }

                    const result = await response.json();

                    if (result.success) {
                        const columnElement = taskCard.closest('.task-list');
                        if (columnElement) {
                            const columnIdPrefix = columnElement.id.split('-')[0];
                            updateTaskCount(columnIdPrefix, -1);
                        }
                        taskCard.remove();
                        console.log('Task deleted:', result.message);
                    } else {
                        console.error('Error deleting task:', result.message || result);
                        alert(`Failed to delete task: ${result.message || 'An error occurred.'}`);
                    }
                } catch (errorCaught) {
                    console.error('Network or JSON parsing error while deleting task:', errorCaught);
                    alert('A network or server error occurred while trying to delete the task.');
                }
            }
        }
    });

    //EDIT TASK
    document.body.addEventListener('click', async function (event) {
        const editButton = event.target.closest('.edit-task-btn');
        if (editButton && IS_ADMIN && editTaskModal && editTaskForm) { 
            event.preventDefault();

            editingTaskId = editButton.dataset.taskId;
            if (!editingTaskId) {
                console.error("Failed to get task ID for editing.");
                if (editTaskFormError) editTaskFormError.textContent = "Task ID not found.";
                return;
            }

            if (editTaskFormError) editTaskFormError.textContent = '';
            editTaskForm.classList.remove('was-validated');

            try {
                const response = await fetch(`/Tasks/Details/${editingTaskId}`);
                if (!response.ok) {
                    throw new Error(`Server error: ${response.status} ${response.statusText}`);
                }

                const taskDetails = await response.json();

                if (taskDetails) { 
                    document.getElementById('editTaskId').value = taskDetails.id;
                    document.getElementById('editTaskTitle').value = taskDetails.title || '';
                    document.getElementById('editTaskDescription').value = taskDetails.description || '';
                    document.getElementById('editTaskAssignedUserId').value = taskDetails.assignedUserId || '';
                    editTaskModal.show();
                } else {
                    console.error("No task data was received for editing or the response did not contain the expected fields.");
                    if (editTaskFormError) editTaskFormError.textContent = "Failed to load task data.";
                }
            } catch (error) {
                console.error('Error while retrieving task data for editing:', error);
                if (editTaskFormError) editTaskFormError.textContent = "Data loading error: " + error.message;
            }
        }      
    });
    if (IS_ADMIN && updateTaskButton && editTaskForm && editTaskModal) {
        updateTaskButton.addEventListener('click', async function () {
            if (!editTaskForm.checkValidity()) {
                editTaskForm.classList.add('was-validated');
                if (editTaskFormError) editTaskFormError.textContent = 'Please complete all required fields.';
                return;
            }
            editTaskForm.classList.remove('was-validated');
            if (editTaskFormError) editTaskFormError.textContent = '';

            const formData = new FormData(editTaskForm);
            let assignedUserIdFromEditForm = formData.get('AssignedUserId');
            if (assignedUserIdFromEditForm === "") {
                assignedUserIdFromEditForm = null;
            }

            const updatedTaskData = {
                id: parseInt(formData.get('Id')),
                title: formData.get('Title'),
                description: formData.get('Description'),
                assignedUserId: assignedUserIdFromEditForm
            };

            try {
                const response = await fetch('/Tasks/Edit', { 
                    method: 'POST', 
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': CSRF_TOKEN
                    },
                    body: JSON.stringify(updatedTaskData)
                });
                const result = await response.json();

                if (response.ok && result.success && result.task) {
                    const existingTaskCard = document.getElementById(`task-${result.task.id}`);
                    if (existingTaskCard) {
                        const updatedTaskCardHtml = createTaskCardHtml(result.task);
                        existingTaskCard.outerHTML = updatedTaskCardHtml;
                    } else {
                        console.warn("No card found for update in DOM, id:", result.task.id);
                    }

                    if (editTaskModal && typeof editTaskModal.hide === 'function') {
                        editTaskModal.hide();
                    }
                    editTaskForm.reset();
                    editTaskForm.classList.remove('was-validated');
                    if (editTaskFormError) editTaskFormError.textContent = '';
                } else {
                    const errorMessage = result.errors ? result.errors.join('; ') : (result.message || 'Failed to update task');
                    if (editTaskFormError) editTaskFormError.textContent = errorMessage;
                    console.error('Error updating task (server response):', result);
                }
            } catch (error) {
                if (editTaskFormError) editTaskFormError.textContent = 'A network or server error occurred while updating.';
                console.error('Network error or JSON parse error (Update Task):', error);
            }
        });
    } else if (IS_ADMIN) {
        console.warn('Could not fully initialize task editing. Check element IDs: editTaskModal, editTaskForm, updateTaskButton.');
    }

    //EDIT TASK STATUS (DRAG AND DROP)
    let draggedItem = null;
    let originalColumnId = null;

    document.querySelector('.kanban-board').addEventListener('dragstart', function(event) {
        const taskCard = event.target.closest('.task-card');
        if (taskCard) {
            draggedItem = taskCard;
            originalColumnId = taskCard.closest('.task-list').id.split('-')[0];
            event.dataTransfer.setData('text/plain', taskCard.dataset.taskId);
            setTimeout(() => {
                taskCard.classList.add('dragging');
            }, 0);
        }
    });

    document.querySelector('.kanban-board').addEventListener('dragend', function(event) {
        const taskCard = event.target.closest('.task-card');
        if (taskCard) {
            taskCard.classList.remove('dragging');
            draggedItem = null;
            originalColumnId = null;
        }
    });

    const taskLists = document.querySelectorAll('.task-list');
    taskLists.forEach(list => {
        list.addEventListener('dragover', function (event) {
            event.preventDefault();
            list.classList.add('drag-over');
        });

        list.addEventListener('dragleave', function (event) {
            list.classList.remove('drag-over');
        });

        list.addEventListener('drop', async function (event) {
            event.preventDefault();
            list.classList.remove('drag-over');

            const taskId = event.dataTransfer.getData('text/plain');
            const elementToMove = document.getElementById(`task-${taskId}`);

            if (!elementToMove) {
                if (draggedItem) draggedItem.classList.remove('dragging');
                draggedItem = null;
                originalColumnId = null;
                return;
            }

            const targetColumnElement = event.target.closest('.task-list');
            if (!targetColumnElement) {
                console.error("DROP: Failed to identify target column.");
                return;
            }

            const newStatusString = targetColumnElement.dataset.status;

            let newStatusEnumValue;
            switch (newStatusString) {
                case 'ToDo': newStatusEnumValue = 1; break;
                case 'InProgress': newStatusEnumValue = 2; break;
                case 'Done': newStatusEnumValue = 3; break;
                default:
                    console.error(`Unknown task status: ${newStatusString}`);
                    return;
            }

            const targetColumnId = targetColumnElement.id.split('-')[0];
            const currentElementOriginalColumnId = elementToMove.closest('.task-list').id.split('-')[0];

            if (currentElementOriginalColumnId === targetColumnId) {
                return;
            }

            try {
                const response = await fetch(`/Tasks/UpdateStatus/${taskId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': CSRF_TOKEN
                    },
                    body: JSON.stringify({ NewStatus: newStatusEnumValue })
                });

                if (!response.ok) {
                    let errorText = `Server error: ${response.status} ${response.statusText}`;
                    try { const errorResult = await response.json(); errorText = errorResult.message || errorText; } catch (e) { }
                    console.error('Status update error:', errorText);
                    alert(`Failed to update status: ${errorText}`);
                    return;
                }

                const result = await response.json();
                if (result.success) {
                    const dropIndicatorInTarget = targetColumnElement.querySelector('.drop-indicator');
                    if (dropIndicatorInTarget) {
                        targetColumnElement.insertBefore(elementToMove, dropIndicatorInTarget); 
                    } else {
                        targetColumnElement.appendChild(elementToMove); 
                    }

                    updateTaskCount(currentElementOriginalColumnId, -1);
                    updateTaskCount(targetColumnId, 1);
                } else {
                    console.error('Status update error:', result.message || result);
                    alert(`Failed to update status: ${result.message || 'Błąd.'}`);
                }
            } catch (errorCaught) {
                console.error('Network/JSON parsing error:', errorCaught);
                alert('Network / server error updating status');
            }
        });
    });

});