import firebase_admin
from firebase_admin import firestore_async
from optimizer import Event, Helper, FilledHelper, Optimize
import asyncio

app = firebase_admin.initialize_app()
db = firestore_async.client()

async def fetch_events():
	eventsRef = db.collection("Department").document("v87avboSu7Dc74ZJpJFk").collection("Event").list_documents()
	events = []
	async for eventRef in eventsRef:
		helpers = await eventRef.collection("Helper").get()
		event = Event([])
		for helperRef in helpers:
			helperSnapshot = helperRef.to_dict()
			if helperRef.read_time < helperSnapshot["LockingTime"]:
				lockedMembers = []
				availableMembers = helperSnapshot["SetMembers"].append(helperSnapshot["SetMembers"])
			else:
				lockedMembers = helperSnapshot["SetMembers"]
				availableMembers = helperSnapshot["SetMembers"]

			event.Helpers.append(Helper(helperRef.id, eventRef.id, helperSnapshot["HelperCategoryId"], helperSnapshot["RequiredAmount"], lockedMembers, availableMembers))
		events.append(event)
	
	filledHelpers = Optimize(events)
	print(filledHelpers)

asyncio.run(fetch_events())
