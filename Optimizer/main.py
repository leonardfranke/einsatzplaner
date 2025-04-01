import firebase_admin
import functions_framework
from firebase_admin import firestore_async
import flask
from optimizer import Event, Helper, Optimize
import asyncio

app = firebase_admin.initialize_app()
db = firestore_async.client()

async def optimizeDepartment(departmentId : str):
	eventsRef = db.collection("Department").document(departmentId).collection("Event")

	events = []
	async for eventRef in eventsRef.list_documents():
		helpers = await eventRef.collection("Helper").get()
		event = Event([])
		for helperRef in helpers:
			helperSnapshot = helperRef.to_dict()
			if helperRef.read_time < helperSnapshot["LockingTime"]:
				lockedMembers = []
				availableMembers = helperSnapshot["SetMembers"].append(helperSnapshot["QueuedMembers"])
			else:
				lockedMembers = helperSnapshot["SetMembers"]
				availableMembers = helperSnapshot["QueuedMembers"]

			event.Helpers.append(Helper(helperRef.id, eventRef.id, helperSnapshot["HelperCategoryId"], helperSnapshot["RequiredAmount"], lockedMembers, availableMembers))
		events.append(event)
	
	filledHelpers = Optimize(events)
	
	for filledHelper in filledHelpers:
		await eventsRef.document(filledHelper.EventId).collection("Helper").document(filledHelper.Id).update({
			"SetMembers": filledHelper.SetMembers,
			"QueuedMembers": filledHelper.RemainingMembers,
		})

@functions_framework.http
def optimize(request : flask.Request):
	departmentId = request.args.get("departmentId")
	if not departmentId:
		return "DepartmentId is required", 400
	return asyncio.run(optimizeDepartment(departmentId))
