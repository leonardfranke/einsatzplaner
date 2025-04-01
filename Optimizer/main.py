import firebase_admin
import functions_framework
from firebase_admin import firestore
import flask
from flask import jsonify
from optimizer import Event, Helper, Optimize
import asyncio

if not firebase_admin._apps:
    firebase_admin.initialize_app()
db = firestore.client()

def optimizeDepartment(departmentId : str):
	eventsRef = db.collection("Department").document(departmentId).collection("Event")

	events = []
	for eventRef in eventsRef.list_documents():
		helpers = eventRef.collection("Helper").get()
		event = Event([])
		for helperRef in helpers:
			helperSnapshot = helperRef.to_dict()
			if helperRef.read_time < helperSnapshot["LockingTime"]:
				lockedMembers = []
				availableMembers = helperSnapshot["SetMembers"] + helperSnapshot["QueuedMembers"]
			else:
				lockedMembers = helperSnapshot["SetMembers"]
				availableMembers = helperSnapshot["QueuedMembers"]

			event.Helpers.append(Helper(helperRef.id, eventRef.id, helperSnapshot["HelperCategoryId"], helperSnapshot["RequiredAmount"], lockedMembers, availableMembers))
		events.append(event)
	
	filledHelpers = Optimize(events)
	
	for filledHelper in filledHelpers:
		eventsRef.document(filledHelper.EventId).collection("Helper").document(filledHelper.Id).update({
			"SetMembers": filledHelper.SetMembers,
			"QueuedMembers": filledHelper.RemainingMembers,
		})

@functions_framework.http
def optimize(request : flask.Request):
	departmentId = request.args.get("departmentId")
	if not departmentId:
		return "DepartmentId is required", 400
	
	optimizeDepartment(departmentId)
	return "Optimization completed", 200
