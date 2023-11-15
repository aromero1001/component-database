const fechaDesde = new Date();



fechaDesde.setDate(fechaDesde.getDate() - 1);



db.NotificationRequests.aggregate([

    {

        $lookup: {

            from: "Notifications",

            localField: "_id",

            foreignField: "RequestId",

            as: "notifications"

        }

    },

    {

        $match: {

            notifications: { $eq: [] },

            CreationDate: { $gte: fechaDesde },

            Done: true

        }

    }

])