import functions_framework
import flask
import traceback
from google.cloud import firestore
from optimizer import Event, Requirement, Qualification, Optimize

db = firestore.Client()

def optimizeDepartment(departmentId : str):
	eventsRef = db.collection("Department").document(departmentId).collection("Event")
	eventDocuments = eventsRef.list_documents()

	no_events = True
	events : list[Event] = []
	for eventRef in eventDocuments:
		eventSnapshot = eventRef.get().to_dict()
		no_events = False
		helpers = eventRef.collection("Helper").get()
		event = Event(Id=eventRef.id,Date=eventSnapshot["Date"], Requirements=[])
		for helperRef in helpers:
			helperSnapshot = helperRef.to_dict()
			try:
				helper = Requirement(helperRef.id, eventRef.id, helperSnapshot["RoleId"], helperSnapshot["LockingTime"], helperSnapshot["RequiredAmount"], helperSnapshot["LockedMembers"], helperSnapshot["PreselectedMembers"], helperSnapshot["AvailableMembers"], helperSnapshot["RequiredQualifications"])
				event.Requirements.append(helper)
			except Exception as e:
				traceback.print_exc()
			
		events.append(event)
	
	if no_events:
		return "No events found in department", 404
	
	relevantQualificationIds = [qualificationId for event in events for requirement in event.Requirements for qualificationId in requirement.RequiredQualifications]
	
	qualificationsRef = db.collection("Department").document(departmentId).collection("Qualification")
	qualificationsSnapshots = [{**docRef.get().to_dict(), "id": docRef.id} for docRef in qualificationsRef.list_documents() if docRef.id in relevantQualificationIds]
	qualifications = [Qualification(Id=qualiSnapshot["id"],RoleId=qualiSnapshot["RoleId"],Members=qualiSnapshot["MemberIds"]) for qualiSnapshot in qualificationsSnapshots]

	print(events)
	print(qualifications)
	filledHelpers = Optimize(events, qualifications)
	
	for filledHelper in filledHelpers:
		oldHelper = filledHelper[0]
		updates = filledHelper[1]

		newLockedMembers = set(updates.NewLockedMembers)
		newPreselectedMembers = set(updates.NewPreselectedMembers)
		newAvailableMembers = set(updates.NewAvailableMembers)

		oldLockedMembers = set(oldHelper.LockedMembers)
		oldPreselectedMembers = set(oldHelper.PreselectedMembers)
		oldAvailableMembers = set(oldHelper.AvailableMembers)

		lockedMembersToAdd = list(newLockedMembers.difference(oldLockedMembers))
		lockedMembersToRemove = list(oldLockedMembers.difference(newLockedMembers))

		preselectedMembersToAdd = list(newPreselectedMembers.difference(oldPreselectedMembers))
		preselectedMembersToRemove = list(oldPreselectedMembers.difference(newPreselectedMembers))

		availableMembersToAdd = list(newAvailableMembers.difference(oldAvailableMembers))
		availableMembersToRemove = list(oldAvailableMembers.difference(newAvailableMembers))

		toAddDict = {}
		if len(lockedMembersToAdd) > 0:
			toAddDict["LockedMembers"] = firestore.ArrayUnion(lockedMembersToAdd)
		if len(preselectedMembersToAdd) > 0:
			toAddDict["PreselectedMembers"] = firestore.ArrayUnion(preselectedMembersToAdd)
		if len(availableMembersToAdd) > 0:
			toAddDict["AvailableMembers"] = firestore.ArrayUnion(availableMembersToAdd)
		if len(toAddDict) > 0:
			eventsRef.document(oldHelper.EventId).collection("Helper").document(oldHelper.Id).update(toAddDict)

		toRemoveDict = {}
		if len(lockedMembersToRemove) > 0:
			toRemoveDict["LockedMembers"] = firestore.ArrayRemove(lockedMembersToRemove)
		if len(preselectedMembersToRemove) > 0:
			toRemoveDict["PreselectedMembers"] = firestore.ArrayRemove(preselectedMembersToRemove)
		if len(availableMembersToRemove) > 0:
			toRemoveDict["AvailableMembers"] = firestore.ArrayRemove(availableMembersToRemove)
		if len(toRemoveDict) > 0:
			eventsRef.document(oldHelper.EventId).collection("Helper").document(oldHelper.Id).update(toRemoveDict)

@functions_framework.http
def optimize(request : flask.Request):
	departmentId = request.args.get("departmentId")
	if not departmentId:
		return "DepartmentId is required", 400
	
	result = optimizeDepartment(departmentId)
	if result:
		return result
	return "Optimization completed", 200

if __name__ == "__main__":
	optimizeDepartment("v87avboSu7Dc74ZJpJFk")